using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class TableDto
    {
        public string TableName { get; set; } = null!;
        public Domain.Enums.TableSize TableSizeId { get; set; }
        public Guid? TableRatingId { get; set; }
    }
}
