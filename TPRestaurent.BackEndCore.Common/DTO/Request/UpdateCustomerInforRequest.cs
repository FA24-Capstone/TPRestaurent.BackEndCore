using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateCustomerInforRequest
    {
        public Guid CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; } = null!;
        public DateTime DOB { get; set; } 
        public string? Address { get; set; } = null!;
        public Guid? AddressId { get; set; }
        public string? AccountId { get; set; }
    }
}
