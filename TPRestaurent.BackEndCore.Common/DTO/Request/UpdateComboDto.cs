using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateComboDto
    {
        public Guid ComboId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IFormFile MainImg { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
        public List<UpdateDishCombo> DishComboDtos { get; set; } = new List<UpdateDishCombo>();
    }
   
    public class UpdateDishCombo
    {
        public Guid DishComboId { get; set; }
        public bool HasOptions { get; set; }
        public int? OptionSetNumber { get; set; }
        public List<Guid> ListDishId { get; set; } = new List<Guid>();
    }
}
