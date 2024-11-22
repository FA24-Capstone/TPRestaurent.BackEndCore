using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IChatBotService
    {
        Task<AppActionResult> ResponseCustomer(ChatbotRequestDto dto);
    }
}