using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ComboResponseDto
    {
        public Combo Combo { get; set; } = null!;
        public double AverageRating { get; set; } = 0;
        public int NumberOfRating { get; set; } = 0;
    }
}
