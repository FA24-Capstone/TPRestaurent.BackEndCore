using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountImageRequest
    {
        public string? AccountId { get; set; }
        public string? Avatar { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}
