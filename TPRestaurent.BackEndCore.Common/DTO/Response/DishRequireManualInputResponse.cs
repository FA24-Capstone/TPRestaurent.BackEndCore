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
        public List<DishSizeDetail> DishSizeDetails { get; set; } = new List<DishSizeDetail>();
        public List<Combo> Combos { get; set; } = new List<Combo>();
    }
}
