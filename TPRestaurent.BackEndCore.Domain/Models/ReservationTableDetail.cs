using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class ReservationTableDetail
    {
        [Key]
        public Guid ReservationTableDetailId { get; set; }
        public Guid TableId { get; set; }
        [ForeignKey(nameof(TableId))]
        public Table? Table { get; set; }
        public Guid ReservationId { get; set; }
        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
    }
}
