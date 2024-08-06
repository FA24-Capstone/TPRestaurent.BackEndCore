using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class StoreCreditHistory
    {
        [Key]
        public Guid StoreCreditHistoryId { get; set; }
        public bool IsInput { get; set; }
        public DateTime? Date {  get; set; }
        public double Amount { get; set; }
        public Guid StoreCreditId { get; set; }
        [ForeignKey(nameof(StoreCreditId))]
        public StoreCredit? StoreCredit { get; set; }
    }
}
