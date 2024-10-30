using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class FindTableDto
    {
        public DateTime StartTime {  get; set; }
        public DateTime? EndTime { get; set; }
        public int Quantity { get; set; }
        public Guid RoomId { get; set; }
    }
}
