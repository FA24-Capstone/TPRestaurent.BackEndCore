using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

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

        Task<AppActionResult> GetUserByRank(UserRank userRank, Guid? couponprogramId, bool? hasBeenProvided);
        Task<AppActionResult> GetTotalUserByRank();

        Task<AppActionResult> AssignCouponToUserWithRank(AssignCouponToRankRequest dto);
        public Task AssignCouponToUserWithRank();
        public Task GetBirthdayUserForCoupon();
        public Task RemoveExpiredCoupon();
        public Task ResetUserRank();
        public Task<string> UpgradeUserRank();
    }
}