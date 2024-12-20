﻿namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateCouponProgramDto
    {
        public Guid CouponProgramId { get; set; }
        public string? Code { get; set; }
        public string? Title { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? MinimumAmount { get; set; }
        public int? Quantity { get; set; }
        public string? ImageFile { get; set; }
        public Domain.Enums.CouponProgramType? CouponProgramType { get; set; }
        public Domain.Enums.UserRank? UserRank { get; set; }
        public string? AccountId { get; set; }
    }
}