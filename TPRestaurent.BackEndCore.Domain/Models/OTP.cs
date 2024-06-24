using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
