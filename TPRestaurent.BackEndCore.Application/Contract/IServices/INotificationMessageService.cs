using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices;

public interface INotificationMessageService
{
    Task<AppActionResult> GetNotificationMessageById(Guid notifiId);
    Task<AppActionResult> GetNotificationMessageByAccountId(string accountId);
    Task<AppActionResult> SendNotificationToAccountAsync(string accountId, string message);
    Task<AppActionResult> SendNotificationToRoleAsync(string roleName, string message);
    Task<AppActionResult> MarkMessageAsRead(List<Guid> messageIds);
}