using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Syntech.WebApp.Models;
using Syntech.WebApp.Services;

namespace Syntech.WebApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return BadRequest("Please enter a question.");
            }

            if (question.Length > 1000)
            {
                return BadRequest(
                    "Your question cannot exceed 1000 characters.");
            }

            var response =
                await _chatbotService.GetResponseAsync(question);

            return Json(new
            {
                success = true,
                response
            });
        }

    }
}
