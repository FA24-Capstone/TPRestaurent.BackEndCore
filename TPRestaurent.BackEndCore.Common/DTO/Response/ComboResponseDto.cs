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
        public Dictionary<int, List<DishCombo>> DishCombo { get; set; } = new Dictionary<int, List<DishCombo>>();
        public List<string> Imgs { get; set; } = new List<string>();
    }

}
