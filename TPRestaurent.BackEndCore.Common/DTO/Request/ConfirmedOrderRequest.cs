﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ConfirmedOrderRequest
    {
        public Guid OrderId { get; set; }
        public bool? IsSuccessful { get; set; } = true;
        public string? CancelReason { get; set; } = string.Empty;
        public IFormFile? Image { get; set; } = null!;
    }
}
