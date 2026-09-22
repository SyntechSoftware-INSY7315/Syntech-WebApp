#pragma warning disable OPENAI001

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAISettings _openAISettings;

        public ChatbotService(
            ApplicationDbContext context,
            IOptions<OpenAISettings> openAISettings)
        {
            _context = context;
            _openAISettings = openAISettings.Value;
        }

        public async Task<string> GetResponseAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return "Please enter a question.";
            }

            var articles = await _context.HelpArticles
                .Where(a => a.IsActive)
                .ToListAsync();

            if (!articles.Any())
            {
                return "I'm sorry, but I don't currently have enough information to answer your question.";
            }

            // Extract meaningful keywords from the client's question
            var keywords = GetKeywords(question);

            // Score every active article based on relevance
            var relevantArticles = articles
                .Select(article => new
                {
                    Article = article,
                    Score = CalculateRelevanceScore(
                        article,
                        keywords,
                        question)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Article)
                .ToList();

            // Do not send unrelated articles to OpenAI
            if (!relevantArticles.Any())
            {
                return "I'm sorry, but I don't currently have enough information to answer your question. Please contact Syntech Software support for further assistance.";
            }

            var knowledgeBase = string.Join(
                "\n\n",
                relevantArticles.Select(a =>
                    $"Title: {a.Title}\n" +
                    $"Category: {a.Category}\n" +
                    $"Content: {a.Content}"));

            var prompt =
                """
                You are the Syntech Software client support assistant.

                Answer the client's question using ONLY the information
                provided in the knowledge base below.

                Do not invent information.

                If the knowledge base does not contain enough information
                to answer the question, clearly say that you do not have
                enough information and recommend that the client contact
                Syntech support.

                Do not claim that you can access private project information,
                documents, support tickets, consultations, passwords, or
                other client-specific data.

                Do not claim that you can perform actions on behalf of
                the client unless the knowledge base explicitly says so.

                Keep your answer clear, helpful, and concise.

                KNOWLEDGE BASE:
                """ + knowledgeBase +
                """

                CLIENT QUESTION:
                """ + question;

            try
            {
                var client = new ResponsesClient(
                    apiKey: _openAISettings.ApiKey);

                var response = await client.CreateResponseAsync(
                    "gpt-5-mini",
                    prompt);

                return response.Value.GetOutputText();
            }
            catch (Exception ex)
            {
                // Handle exhausted API credits
                if (IsCreditBalanceError(ex))
                {
                    return "The AI assistant is currently unavailable. Please try again later or contact Syntech Software support for assistance.";
                }

                // Handle other OpenAI/API failures without exposing technical details to the client
                return "The AI assistant is currently unavailable. Please try again later or contact Syntech Software support for assistance.";
            }
        }

        private static List<string> GetKeywords(string question)
        {
            var cleanedQuestion = new string(
                question
                    .ToLowerInvariant()
                    .Select(c =>
                        char.IsLetterOrDigit(c) ||
                        char.IsWhiteSpace(c)
                            ? c
                            : ' ')
                    .ToArray());

            var words = cleanedQuestion
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var stopWords = new HashSet<string>
            {
                "a",
                "an",
                "and",
                "are",
                "can",
                "could",
                "do",
                "does",
                "for",
                "how",
                "i",
                "if",
                "in",
                "is",
                "it",
                "me",
                "my",
                "of",
                "on",
                "the",
                "to",
                "what",
                "when",
                "where",
                "who",
                "why",
                "with",
                "you",
                "your"
            };

            return words
                .Where(word => word.Length >= 2)
                .Where(word => !stopWords.Contains(word))
                .Distinct()
                .ToList();
        }

        private static int CalculateRelevanceScore(
            HelpArticle article,
            List<string> keywords,
            string question)
        {
            int score = 0;

            var title = article.Title.ToLowerInvariant();
            var category =
                article.Category?.ToLowerInvariant() ?? string.Empty;

            var content = article.Content.ToLowerInvariant();

            var lowerQuestion = question.ToLowerInvariant();

            // Exact question phrase in title
            if (title.Contains(lowerQuestion))
            {
                score += 20;
            }

            // Exact question phrase in content
            if (content.Contains(lowerQuestion))
            {
                score += 5;
            }

            foreach (var keyword in keywords)
            {
                // Direct keyword matches
                if (title.Contains(keyword))
                {
                    score += 5;
                }

                if (category.Contains(keyword))
                {
                    score += 3;
                }

                if (content.Contains(keyword))
                {
                    score += 1;
                }

                // Related keyword matches
                var relatedTerms = GetRelatedTerms(keyword);

                foreach (var relatedTerm in relatedTerms)
                {
                    if (title.Contains(relatedTerm))
                    {
                        score += 4;
                    }

                    if (category.Contains(relatedTerm))
                    {
                        score += 2;
                    }

                    if (content.Contains(relatedTerm))
                    {
                        score += 1;
                    }
                }
            }

            return score;
        }

        private static List<string> GetRelatedTerms(string keyword)
        {
            var termGroups = new List<string[]>
            {
                // Consultation / booking
                new[]
                {
                    "book",
                    "booking",
                    "schedule",
                    "scheduled",
                    "appointment",
                    "meeting"
                },

                // Documents / files
                new[]
                {
                    "document",
                    "documents",
                    "file",
                    "files",
                    "upload",
                    "uploads",
                    "uploaded",
                    "uploading",
                    "download",
                    "downloads",
                    "downloaded"
                },

                // Support tickets
                new[]
                {
                    "ticket",
                    "tickets",
                    "support",
                    "issue",
                    "issues",
                    "problem",
                    "problems"
                },

                // Projects / progress
                new[]
                {
                    "project",
                    "projects",
                    "progress",
                    "status",
                    "statuses",
                    "update",
                    "updates"
                },

                // Account / authentication
                new[]
                {
                    "account",
                    "login",
                    "log",
                    "password",
                    "access",
                    "permission",
                    "permissions"
                },

                // AI chatbot
                new[]
                {
                    "ai",
                    "assistant",
                    "chatbot",
                    "chat",
                    "bot"
                },

                // Services / software
                new[]
                {
                    "service",
                    "services",
                    "software",
                    "development",
                    "develop"
                }
            };

            foreach (var group in termGroups)
            {
                if (group.Contains(keyword))
                {
                    return group
                        .Where(term => term != keyword)
                        .ToList();
                }
            }

            return new List<string>();
        }

        private static bool IsCreditBalanceError(Exception exception)
        {
            var message = exception.ToString().ToLowerInvariant();

            return message.Contains("credit_balance_exhausted") ||
                   message.Contains("credit balance exhausted") ||
                   message.Contains("insufficient_quota") ||
                   message.Contains("no credits remaining");
        }

    }
}
