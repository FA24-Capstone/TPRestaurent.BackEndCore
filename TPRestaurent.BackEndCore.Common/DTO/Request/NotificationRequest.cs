using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Common.DTO.Request;

public class NotificationRequest
{
    public string DeviceToken { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public AppActionResult Data { get; set; }
}