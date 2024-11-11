using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateRatingRequestDto
    {
        public Guid RatingId { get; set; }
        public string? Title { get; set; } = null!;
        public Domain.Enums.RatingPoint PointId { get; set; }
        public string? Content { get; set; } = null!;
        public List<IFormFile>? ImageFiles { get; set; } = new List<IFormFile>();
    }
}
