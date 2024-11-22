using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class OTP
    {
        [Key]
        public Guid OTPId { get; set; }

        public OTPType Type { get; set; }
        public string Code { get; set; } = null!;
        public DateTime ExpiredTime { get; set; }
        public bool IsUsed { get; set; }
        public string? AccountId { get; set; } = null!;

        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
    }
}