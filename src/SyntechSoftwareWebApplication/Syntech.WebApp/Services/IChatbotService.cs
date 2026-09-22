namespace Syntech.WebApp.Services
{
    public interface IChatbotService
    {
        Task<string> GetResponseAsync(string question);
    }
}
