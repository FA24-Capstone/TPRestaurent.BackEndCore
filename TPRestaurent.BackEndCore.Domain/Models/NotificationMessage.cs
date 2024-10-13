
using System.ComponentModel.DataAnnotations;

namespace TPRestaurent.BackEndCore.Domain.Models;

public class NotificationMessage
{
    [Key] 
    public Guid NotificationId { get; set; }
    public string NotificationName { get; set; }
    public string Messages { get; set; }
}