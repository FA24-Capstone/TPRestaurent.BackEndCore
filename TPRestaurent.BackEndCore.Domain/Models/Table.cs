using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Table
    {
        [Key]
        public Guid TableId { get; set; }
        public string TableName { get; set; } = null!;
        public Enums.TableSize TableSizeId { get; set; }
        [ForeignKey(nameof(TableSizeId))]
        public EnumModels.TableSize? TableSize { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? TableRatingId { get; set; }
        [ForeignKey(nameof(TableRatingId))]
        public TableRating? TableRating { get; set; }
        public Guid? OrderDetailId { get; set; }
        [ForeignKey(nameof(OrderDetailId))]
        public OrderDetail? OrderDetail { get; set; }
    }
}
