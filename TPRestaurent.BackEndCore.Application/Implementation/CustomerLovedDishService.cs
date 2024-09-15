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
    //public class CustomerLovedDishService : GenericBackendService, ICustomerLovedDishService
    //{
    //    private readonly IGenericRepository<CustomerLovedDish> _repository;
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IMapper _mapper;
    //    public CustomerLovedDishService(IGenericRepository<CustomerLovedDish> repository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service): base(service)
    //    {
    //        _repository = repository;
    //        _unitOfWork = unitOfWork;
    //        _mapper = mapper;
    //    }

    //    public async Task<AppActionResult> GetAllCustomerLovedDish(string accountId, int pageNumber, int pageSize)
    //    {
    //        AppActionResult result = new AppActionResult();
    //        try
    //        {
    //            var couponDb = await _repository.GetAllDataByExpression(c => c.CustomerInfo.AccountId.Equals(accountId), pageNumber, pageSize, null, false, c => c.Dish, c => c.Combo);
    //            result.Result = couponDb;
    //        }
    //        catch (Exception ex)
    //        {
    //            result = BuildAppActionResultError(result, ex.Message);
    //        }
    //        return result;
    //    }
    //}
}
