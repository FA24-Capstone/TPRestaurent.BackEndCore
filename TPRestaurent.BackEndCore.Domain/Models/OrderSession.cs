using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class OrderSession
    {
        [Key]
        public Guid OrderSessionId { get; set; }
        public DateTime OrderSessionTime { get; set; }  
        public int OrderSessionNumber { get; set; } 
        public Enums.OrderSessionStatus OrderSessionStatusId { get; set; }
        [ForeignKey(nameof(OrderSessionStatusId))]
        public EnumModels.OrderSessionStatus? OrderSessionStatus { get; set; }   

    }
}
