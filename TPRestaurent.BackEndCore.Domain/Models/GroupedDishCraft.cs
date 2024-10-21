using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class GroupedDishCraft
    {
        public Guid GroupedDishCraftId { get; set; }
        public DateTime StartTime { get; set; }
        public string OrderDetailIdList { get; set; }
        public string GroupedDishJson { get; set; }
    }
}
