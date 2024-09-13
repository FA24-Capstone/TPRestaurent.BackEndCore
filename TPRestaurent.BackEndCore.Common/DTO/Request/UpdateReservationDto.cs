using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateReservationDto
    {
        public Guid ReservationId { get; set; }
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime? EndTime { get; set; }
        public double? AdditionalDeposit { get; set; }
        public List<Guid> ReservationTableIds { get; set; } = new List<Guid>();
    }
}
