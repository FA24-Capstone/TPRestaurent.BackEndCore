﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class ReservationDish
    {
        [Key]
        public Guid ReservationDishId { get; set; }
        public Guid? ReservationId { get; set; }
        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
        public Guid? DishSizeDetailId { get; set; }
        [ForeignKey(nameof(DishSizeDetailId))]
        public DishSizeDetail? DishSizeDetail { get; set; }
        public Guid? ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; } 
    }
}
