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
    public class DishCombo
    {
        [Key]
        public Guid DishComboId { get; set; }
        public bool HasOptions { get; set; }
        public int? OptionSetNumber { get; set; }
        public int Quantity { get; set; }
        public Guid DishSizeDetailId { get; set; }
        [ForeignKey(nameof(DishSizeDetailId))]
        public DishSizeDetail? DishSizeDetail { get; set; }
        public Guid ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
    }
}
