using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices;

public interface INotificationMessageService
{
    Task<AppActionResult> GetNotificationMessageById(Guid notifiId);
}