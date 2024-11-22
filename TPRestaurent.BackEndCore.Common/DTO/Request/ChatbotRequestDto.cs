namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ChatbotRequestDto
    {
        public string? CustomerId { get; set; }
        public string? Message { get; set; }
        public bool IsFirstCall { get; set; }
    }
}