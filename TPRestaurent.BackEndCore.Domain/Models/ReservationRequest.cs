using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models.BaseModel;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class ReservationRequest : BaseEntity
    {
        [Key]
        public Guid ReservationRequestId { get; set; }
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime? EndTime { get; set; }
        public Enums.ReservationRequestStatus Status { get; set; }
        [ForeignKey(nameof(Status))]
        public ReservationRequestStatus ReservationRequestStatus { get; set; }
        public string? Note { get; set; }
        public string? ReservationDishes { get; set; }
    }
}
