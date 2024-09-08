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
        public Guid? CustomerInfoId { get; set; }
        public double Deposit { get; set; }
        public string? Note { get; set; }
        public bool IsPrivate { get; set; }
        //public List<Guid> ReservationTableIds { get; set; } = new List<Guid>();
    }

    
}
