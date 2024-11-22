using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices;

public interface INotificationMessageService
{
    Task<AppActionResult> GetNotificationMessageById(Guid notifiId);

    Task<AppActionResult> GetNotificationMessageByAccountId(string accountId);

    Task<AppActionResult> SendNotificationToAccountAsync(string accountId, string message, bool ignoreSaveChanges = false);

    Task<AppActionResult> SendNotificationToRoleAsync(string roleName, string message);

    Task<AppActionResult> SendNotificationToShipperAsync(string accountId, string message);

    Task<AppActionResult> MarkAllMessageAsRead(string accountId);

    Task<AppActionResult> MarkMessageAsRead(Guid notificationId);
}