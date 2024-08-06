using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ReservationReponse
    {
        public Reservation Reservation { get; set; }
        public List<ReservationDish> ReservationDishes { get; set; } = new List<ReservationDish>();
        public List<ReservationTableDetail> ReservationTableDetails { get; set; } = new List<ReservationTableDetail>();
    }
}
