using Microsoft.AspNetCore.Identity;
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
        public string Id { get; set; } = null!;
        public Account CustomerInfo { get; set; }
        public List<CustomerInfoAddress> CustomerAddresses { get; set; } = new List<CustomerInfoAddress>();
        public double StoreCredit { get; set; }
        public DateTime StoreCreditExpireDay { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public string MainRole { get; set; }
    }
}
