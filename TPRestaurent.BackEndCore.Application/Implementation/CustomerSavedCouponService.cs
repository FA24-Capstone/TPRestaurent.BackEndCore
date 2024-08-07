using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class CustomerSavedCouponService : GenericBackendService, ICustomerSavedCouponService
    {
        private readonly IGenericRepository<CustomerSavedCoupon> _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public CustomerSavedCouponService(IGenericRepository<CustomerSavedCoupon> repository, IMapper mapper, IUnitOfWork unitOfWork, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> GetAllCustomerCoupon(string accountId, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var couponDb = await _repository.GetAllDataByExpression(c => c.CustomerInfo.AccountId.Equals(accountId) && !c.IsUsedOrExpired, pageNumber, pageSize, c => c.Coupon.ExpiryDate, true, c => c.Coupon);
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
    }
}
