using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class SearchDishInfo
    {
        public List<string> Tags { get; set; }
        public string Name { get; set; }
        public (decimal? Min, decimal? Max) PriceRange { get; set; }
    }
}
