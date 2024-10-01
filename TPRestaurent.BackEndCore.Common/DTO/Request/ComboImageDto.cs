using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ComboImageDto
    {
        public Guid ComboId { get; set; }
        public IFormFile MainImg { get; set; }
        public List<IFormFile>? Imgs { get; set; } = new List<IFormFile> ();
    }
}
