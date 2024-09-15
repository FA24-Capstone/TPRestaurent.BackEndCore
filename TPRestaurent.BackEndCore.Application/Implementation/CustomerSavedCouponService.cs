using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class CustomerSavedCouponService : GenericBackendService, ICustomerSavedCouponService
    {
        private readonly IGenericRepository<CustomerSavedCoupon> _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private BackEndLogger _logger;
        public CustomerSavedCouponService(IGenericRepository<CustomerSavedCoupon> repository, IMapper mapper, IUnitOfWork unitOfWork, IServiceProvider service, BackEndLogger logger) : base(service)
        {
            _repository = repository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;   
        }

        public async Task<AppActionResult> GetAllCustomerCoupon(string accountId, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var couponDb = await _repository.GetAllDataByExpression(c => c.AccountId.Equals(accountId) && !c.IsUsedOrExpired, pageNumber, pageSize, c => c.Coupon.ExpiryDate, true, c => c.Coupon);
                result.Result = couponDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> TakeCoupon(string accountId, Guid couponId)
        {
            AppActionResult result = new AppActionResult();
            try
            {

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public async Task UpdateExpiredCouponStatus()
        {
            var utility = Resolve<Utility>();
            var couponRepository = Resolve<IGenericRepository<CouponProgram>>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var customerSaveCouponsDb = await _repository.GetAllDataByExpression(p => p.Coupon!.ExpiryDate < currentTime, 0, 0, null, false, null);
                if (customerSaveCouponsDb!.Items!.Count > 0 && customerSaveCouponsDb.Items != null)
                {
                    foreach (var customerSaveCoupon in customerSaveCouponsDb.Items)
                    {
                        customerSaveCoupon.IsUsedOrExpired = true;
                    }
                    await _repository.UpdateRange(customerSaveCouponsDb.Items);
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }
    }
}
