using System.ComponentModel.DataAnnotations;

namespace TPRestaurent.BackEndCore.Domain.Models.EnumModels
{
    public class PaymentMethod
    {
        [Key]
        public TPRestaurent.BackEndCore.Domain.Enums.PaymentMethod Id { get; set; }

        public string Name { get; set; } = null!;
        public string? VietnameseName { get; set; } = null!;
    }
}