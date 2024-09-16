using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DishSizeResponse
    {
        public Dish Dish { get; set; } = null!;
        public List<DishSizeDetail> dishSizeDetails { get; set; }  = new List<DishSizeDetail>();   
        public double AverageRating { get; set; } = 0;
        public int NumberOfRating { get; set; } = 0;
    }
}
