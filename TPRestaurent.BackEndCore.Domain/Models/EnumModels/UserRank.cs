﻿using System.ComponentModel.DataAnnotations;

namespace TPRestaurent.BackEndCore.Domain.Models.EnumModels
{
    public class UserRank
    {
        [Key]
        public TPRestaurent.BackEndCore.Domain.Enums.UserRank Id { get; set; }

        public string Name { get; set; } = null!;
        public string? VietnameseName { get; set; } = null!;
    }
}