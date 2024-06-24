using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class CustomerInfo
    {
        [Key]
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int LoyaltyPoint { get; set; }
        public string? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }

    }
}
