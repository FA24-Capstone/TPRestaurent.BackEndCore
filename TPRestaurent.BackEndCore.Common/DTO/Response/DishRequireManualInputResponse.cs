using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DishRequireManualInputResponse
    {
        public List<DishDetailRequireManualInputResponse> DishSizeDetails { get; set; } = new List<DishDetailRequireManualInputResponse>();
        public List<Combo> Combos { get; set; } = new List<Combo>();
    }

    public class DishDetailRequireManualInputResponse
    {
        public Dish Dish { get; set; }
        public List<DishSizeDetail> DishSizeDetails { get; set; } = new List<DishSizeDetail>();
    }
}
