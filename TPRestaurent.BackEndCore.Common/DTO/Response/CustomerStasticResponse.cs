using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class CustomerStasticResponse
    {
        public int NumberOfCustomer { get; set; }   
        public double PercentIncrease { get; set; }
    }
}
