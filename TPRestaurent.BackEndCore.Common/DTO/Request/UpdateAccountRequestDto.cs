﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountPhoneNumberRequest
    {
        public string? PhoneNumber { get; set; }
        public string? OTPCode { get; set; }    
    }
}
