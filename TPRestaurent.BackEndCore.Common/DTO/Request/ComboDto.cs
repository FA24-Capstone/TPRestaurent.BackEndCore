using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ComboDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid> DishIds { get; set; } = new List<Guid>();
        public List<OptionItem> OptionItems { get; set; } = new List<OptionItem>();
    }

    public class OptionItem
    {
        public List<Guid> OptionItemDishIds { get; set; } = new List<Guid>();
    }
}
