using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ComboDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DishComboDto> DishComboDtos { get; set; } = new List<DishComboDto>();
        public List<IFormFile>? ImageFiles { get; set; }
    }

    public class DishComboDto
    {
        public bool HasOptions { get; set; }
        public int? OptionSetNumber { get; set; }
        public List<Guid> ListDishId { get; set; } = new List<Guid>();  
    }
}
