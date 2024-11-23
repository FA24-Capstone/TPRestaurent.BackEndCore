using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ISmsService
    {
        Task<AppActionResult> SendMessage(string message, string phoneNumber);
    }
}