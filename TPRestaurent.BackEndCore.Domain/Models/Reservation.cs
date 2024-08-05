using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Reservation
    {
        [Key]
        public Guid ReservationId { get; set; }
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime? EndTime { get; set; }
        public string CustomerAccountId { get; set; } = null!;
        [ForeignKey(nameof(CustomerAccountId))]
        public Account? CustomerAccount { get; set; }
        public double Deposit { get; set; }
        public Enums.ReservationStatus StatusId { get; set; }
        [ForeignKey(nameof(StatusId))]
        public EnumModels.ReservationStatus? ReservationStatus { get; set; }
    }
}
