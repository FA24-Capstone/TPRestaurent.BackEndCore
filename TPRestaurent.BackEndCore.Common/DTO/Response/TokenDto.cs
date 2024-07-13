using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TokenDto
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
