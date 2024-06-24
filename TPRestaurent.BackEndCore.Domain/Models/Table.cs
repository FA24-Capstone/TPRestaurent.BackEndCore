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
        public int Capacity { get; set; }
        public Guid? TableRatingId { get; set; }
        [ForeignKey(nameof(TableRatingId))]
        public TableRating? TableRating { get; set; }
    }
}
