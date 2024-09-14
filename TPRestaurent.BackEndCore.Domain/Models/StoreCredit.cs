using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class StoreCredit
    {
        [Key]
        public Guid StoreCreditId { get; set; }
        public double Amount { get; set; }
        public DateTime ExpiredDate { get; set; }
        public Guid? CustomerInfoId { get; set; }
        [ForeignKey(nameof(CustomerInfoId))]
        public CustomerInfo? CustomerInfo { get; set; }
    }
}
