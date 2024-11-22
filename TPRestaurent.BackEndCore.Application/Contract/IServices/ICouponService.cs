using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ICouponService
    {
        Task<AppActionResult> GetAllAvailableCouponProgram(int pageNumber, int pageSize);
        Task<AppActionResult> GetCouponProgramById(Guid couponId);
        Task<AppActionResult> GetAvailableCouponByAccountId(string accountId, double? total, int pageNumber, int pageSize);
        Task<AppActionResult> CreateCouponProgram(CouponProgramDto couponDto);
        Task<AppActionResult> AssignCoupon(AssignCouponRequestDto couponDto);
        Task<AppActionResult> DeleteCouponProgram(Guid couponId);
        Task<AppActionResult> UpdateCouponProgram(UpdateCouponProgramDto updateCouponDto);
        Task<AppActionResult> GetRanks();
        Task<AppActionResult> GetUserByRank(UserRank userRank);
        Task<AppActionResult> AssignCouponToUserWithRank(AssignCouponToRankRequest dto);
        Task AssignCouponToUserWithRank();
        Task GetBirthdayUserForCoupon();
        Task RemoveExpiredCoupon();
        Task ResetUserRank();
    }
}
