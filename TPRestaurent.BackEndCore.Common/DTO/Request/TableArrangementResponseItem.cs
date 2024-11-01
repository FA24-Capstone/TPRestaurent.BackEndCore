using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class TableArrangementResponseItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Position? Position { get; set; } = new Position();
        public TableSize TableSizeId { get; set; }
        public Guid RoomId { get; set; }
        public Room? Room { get; set; } 
        public Domain.Enums.TableStatus TableStatusId { get; set; }
        //public Domain.Models.EnumModels.TableStatus? TableStatus { get; set; }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
