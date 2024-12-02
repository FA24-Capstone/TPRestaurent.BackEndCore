using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class OTP
    {
        public OTPType Type { get; set; }
        public string Code { get; set; } = null!;
        public DateTime ExpiredTime { get; set; }
    }
}