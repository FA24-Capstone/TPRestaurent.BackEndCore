using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountInfoRequest
    {
        public string? AccountId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DOB { get; set; }
        public string? Address { get; set; } = null!;
        public Guid? AddressId { get; set; }
        public string? Avatar { get; set; }
    }
}
