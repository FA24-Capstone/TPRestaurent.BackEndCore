namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IOpenAIClient
    {
        Task<string> CreateChatCompletion(string userMessage);
    }
}