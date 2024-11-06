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
    public class OrderDetail
    {
        [Key]
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; } 
        public Guid? DishSizeDetailId { get; set; }
        [ForeignKey(nameof(DishSizeDetailId))]
        public DishSizeDetail? DishSizeDetail { get; set; }
        public Guid? ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string? Note { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? HandOverTime { get; set; }
        public DateTime? StartProcessingTime { get; set; }
        public DateTime? ReadyToServeTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public double PreparationTime { get; set; }
        public OrderDetailStatus OrderDetailStatusId { get; set; }
        [ForeignKey(nameof(OrderDetailStatusId))]
        public EnumModels.OrderDetailStatus? OrderDetailStatus { get; set; }     
        public Guid? OrderSessionId { get; set; }    
        [ForeignKey(nameof(OrderSessionId))]
        public OrderSession? OrderSession { get; set; }  

    }
}
