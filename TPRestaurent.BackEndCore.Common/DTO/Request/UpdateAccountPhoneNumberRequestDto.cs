﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountPhoneNumberRequestDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string OTPCode { get; set; } = null!;
    }
}
