public interface IChatService
{
    Task<string> GetChatResponse(string userQuestion);
} 