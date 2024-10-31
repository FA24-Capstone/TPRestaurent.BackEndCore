
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models;

public class NotificationMessage
{
    [Key] 
    public Guid NotificationId { get; set; }
    public string NotificationName { get; set; }
    public string Messages { get; set; }
    public DateTime NotifyTime { get; set; }
    public bool IsRead { get; set; }
    public string AccountId { get; set; }
    [ForeignKey(nameof(AccountId))]
    public Account? Account { get; set; }
}