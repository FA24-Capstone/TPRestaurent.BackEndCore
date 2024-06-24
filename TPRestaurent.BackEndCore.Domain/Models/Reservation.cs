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
        public Guid? CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public CustomerInfo? Customer { get; set; }
        public Guid? TableId { get; set; }
        [ForeignKey(nameof(TableId))]
        public Table? Table { get; set; }
        public double Deposit { get; set; }
    }
}
