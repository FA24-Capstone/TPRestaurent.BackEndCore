using System.ComponentModel.DataAnnotations;

namespace TPRestaurent.BackEndCore.Domain.Models.EnumModels
{
    public class ComboCategory
    {
        [Key]
        public TPRestaurent.BackEndCore.Domain.Enums.ComboCategory Id { get; set; }

        public string Name { get; set; } = null!;
        public string? VietnameseName { get; set; } = null!;
    }
}