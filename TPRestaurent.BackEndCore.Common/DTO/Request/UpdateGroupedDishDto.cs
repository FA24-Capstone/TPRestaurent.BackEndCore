using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateGroupedDishDto
    {
        public Guid GroupedDishId { get; set; }
        public Guid DishId { get; set; }
        public DishSize? Size { get; set; }
    }
}
