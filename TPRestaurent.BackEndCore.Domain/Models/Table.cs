using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Table
    {
        [Key]
        public Guid TableId { get; set; }

        public string TableName { get; set; } = null!;
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;
        public Enums.TableSize TableSizeId { get; set; }

        [ForeignKey(nameof(TableSizeId))]
        public EnumModels.TableSize? TableSize { get; set; }

        public TableStatus TableStatusId { get; set; }

        [ForeignKey(nameof(TableStatusId))]
        public EnumModels.TableStatus? TableStatus { get; set; }

        public string? Coordinates { get; set; }
        public bool IsDeleted { get; set; }
        public Guid RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public Room? Room { get; set; }
    }
}