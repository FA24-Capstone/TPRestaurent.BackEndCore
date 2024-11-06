using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class BestSellerResponse
    {
        public List<Dish> Dishes { get; set; }  = new List<Dish>(); 
        public List<Combo> Combos { get; set; } = new List<Combo>();    

    }
}
