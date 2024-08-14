using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum OTPType
    {
        Login = 0,
        Register = 1,
        ForgotPassword = 2,
        ChangePassword = 3,
        ChangeEmail = 4,
        ChangePhone = 5,
        ConfirmEmail = 6,
        ConfirmPhone = 7,
        ConfirmPayment = 8,
        VerifyForReservation = 9
    }
}
