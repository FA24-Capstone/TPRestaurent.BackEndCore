using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class CustomerInfoAddressResponse
    {
        public Account CustomerInfo { get; set; }
        public List<CustomerInfoAddress> CustomerAddresses { get; set; } = new List<CustomerInfoAddress>();
    }
}
