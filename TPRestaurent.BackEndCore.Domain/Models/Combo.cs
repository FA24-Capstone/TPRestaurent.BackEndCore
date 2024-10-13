using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Combo
    {
        [Key]
        public Guid ComboId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public string? Image {  get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public Enums.ComboCategory CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public EnumModels.ComboCategory? Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
    }
}
