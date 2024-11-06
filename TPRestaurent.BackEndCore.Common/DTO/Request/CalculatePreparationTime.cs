using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CalculatePreparationTime
    {
        public double PreparationTime { get; set; }
        public int Quantity { get; set; }
    }
}
