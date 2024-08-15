using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CalculateDepositRequest
    {
        public bool IsPrivate { get; set; }
        public List<ReservationDishDto> reservationDishDtos { get; set; } = new List<ReservationDishDto>();
    }
}
