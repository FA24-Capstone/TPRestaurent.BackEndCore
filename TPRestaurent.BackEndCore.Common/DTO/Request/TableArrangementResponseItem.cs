using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class TableArrangementResponseItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Position? Position { get; set; } = new Position();
        public TableSize TableSizeId { get; set; }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
