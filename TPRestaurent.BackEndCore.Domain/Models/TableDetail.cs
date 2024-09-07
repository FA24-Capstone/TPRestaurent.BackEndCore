using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class TableDetail
    {
        [Key]
        public Guid TableDetailId { get; set; }
        public Guid TableId { get; set; }
        [ForeignKey(nameof(TableId))]
        public Table? Table { get; set; }
        public Guid OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
    }
}
