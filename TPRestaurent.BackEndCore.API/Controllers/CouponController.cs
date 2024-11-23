﻿using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("coupon")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet("get-available-coupon-program/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllAvailableCouponProgram(int pageNumber = 1, int pageSize = 10)
        {
            return await _couponService.GetAllAvailableCouponProgram(pageNumber, pageSize);
        }

        [HttpGet("get-coupon-program-by-id/{couponId}")]
        public async Task<AppActionResult> GetCouponProgramById(Guid couponId)
        {
            return await _couponService.GetCouponProgramById(couponId);
        }

        [HttpGet("get-ranks")]
        public async Task<AppActionResult> GetRanks()
        {
            return await _couponService.GetRanks();
        }

        [HttpGet("get-user-by-rank")]
        public async Task<AppActionResult> GetUserByRank(UserRank userRank, Guid? couponprogramId, bool hasBeenProvided)
        {
            return await _couponService.GetUserByRank(userRank, couponprogramId, hasBeenProvided);
        }

        [HttpGet("get-total-user-by-rank")]
        public async Task<AppActionResult> GetTotalUserByRank()
        {
            return await _couponService.GetTotalUserByRank();
        }

        [HttpPost("create-coupon-program")]
        public async Task<AppActionResult> CreateCouponProgram(CouponProgramDto couponDto)
        {
            return await _couponService.CreateCouponProgram(couponDto);
        }

        [HttpPost("assign-coupon")]
        public async Task<AppActionResult> AssignCoupon(AssignCouponRequestDto couponDto)
        {
            return await _couponService.AssignCoupon(couponDto);
        }

        [HttpPost("assign-coupon-with-rank")]
        public async Task<AppActionResult> AssignCouponToUserWithRank(AssignCouponToRankRequest couponDto)
        {
            return await _couponService.AssignCouponToUserWithRank(couponDto);
        }

        [HttpGet("get-available-coupon-by-account-id/{accountId}/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAvailableCouponByAccountId(string accountId, double? total, int pageNumber = 1, int pageSize = 10)
        {
            return await _couponService.GetAvailableCouponByAccountId(accountId, total, pageNumber, pageSize);
        }

        [HttpPut("delete-coupon-program")]
        public async Task<AppActionResult> DeleteCouponProgram(Guid couponId)
        {
            return await _couponService.DeleteCouponProgram(couponId);
        }

        [HttpPut("update-coupon")]
        public async Task<AppActionResult> UpdateCoupon(UpdateCouponProgramDto updateCouponDto)
        {
            return await _couponService.UpdateCouponProgram(updateCouponDto);
        }
    }
}