﻿using System.ComponentModel.DataAnnotations;

namespace TPRestaurent.BackEndCore.Domain.Models.EnumModels
{
    public class OTPType
    {
        [Key]
        public TPRestaurent.BackEndCore.Domain.Enums.OTPType Id { get; set; }

        public string Name { get; set; } = null!;
        public string? VietnameseName { get; set; } = null!;
    }
}