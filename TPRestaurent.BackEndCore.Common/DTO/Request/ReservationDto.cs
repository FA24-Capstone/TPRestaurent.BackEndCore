using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ReservationDto
    {
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime? EndTime { get; set; }
        public Guid? CustomerAccountId { get; set; }
        public double Deposit { get; set; }
        public List<ReservationDishDto> ReservationDishDtos { get; set; } = new List<ReservationDishDto>();
        public List<Guid> ReservationTableIds { get; set; } = new List<Guid>();
    }
}
