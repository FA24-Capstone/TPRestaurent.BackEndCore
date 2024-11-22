using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private IFirebaseService _firebase;
        private INotificationMessageService _notificationMessageService;

        public NotificationController(IFirebaseService firebase, INotificationMessageService notificationMessageService)
        {
            _firebase = firebase;
            _notificationMessageService = notificationMessageService;
        }

        [HttpGet("get-all-notification-by-account-id/{accountId}")]
        public async Task<AppActionResult> GetNotificationByAccountId(string accountId)
        {
            return await _notificationMessageService.GetNotificationMessageByAccountId(accountId);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            try
            {
                var result = await _firebase.SendNotificationAsync(
                    request.DeviceToken,
                    request.Title,
                    request.Body,
                   request.Data
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("send-multi")]
        public async Task<IActionResult> SendMulticastAsync([FromBody] List<NotificationRequest> request)
        {
            try
            {
                var result = await _firebase.SendMulticastAsync(request.Select(a => a.DeviceToken).ToList(), "test", "test"
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("mark-all-as-read/{accountId}")]
        public async Task<AppActionResult> MarkAllMessageAsRead(string accountId)
        {
            return await _notificationMessageService.MarkAllMessageAsRead(accountId);
        }

        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<AppActionResult> MarkMessageAsRead(Guid notificationId)
        {
            return await _notificationMessageService.MarkMessageAsRead(notificationId);
        }
    }
}