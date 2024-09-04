using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models.EnumModels
{
    public class TransationStatus
    {
        [Key]
        public TPRestaurent.BackEndCore.Domain.Enums.TransationStatus Id { get; set; }
        public string Name { get; set; } = null!;
        public string? VietnameseName { get; set; } = null!;
    }
}
