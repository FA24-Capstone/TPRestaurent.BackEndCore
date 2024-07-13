using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = null!;
        public string? RecoveryCode { get; set; }
        public string? NewPassword { get; set; }
    }
}
