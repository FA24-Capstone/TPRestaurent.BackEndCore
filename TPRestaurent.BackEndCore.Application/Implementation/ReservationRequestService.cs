using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ReservationRequestService : GenericBackendService, IReservationRequestService
    {
        private readonly IGenericRepository<ReservationRequest> _repository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public ReservationRequestService(IGenericRepository<ReservationRequest> repository,
                                         IUnitOfWork unitOfWork,
                                         IMapper mapper, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppActionResult> CreateReservationRequest(ReservationRequestDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                if(dto.ReservationDate.AddHours(1) < utility.GetCurrentDateTimeInTimeZone())
                {
                    result = BuildAppActionResultError(result, "Thời gian đặt bàn không hợp lệ");
                    return result;
                }   

                if(dto.NumberOfPeople < 1)
                {
                    result = BuildAppActionResultError(result, "Cần ít nhất 1 người dùng bữa để đặt bàn");
                    return result;
                }

                var accountRepository = Resolve<IGenericRepository<Account>>();
                if((await accountRepository!.GetById(dto.CustomerAccountId)) == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng với id {dto.CustomerAccountId}");
                    return result;
                }

                if (dto.ReservationDishes.Count > 0) 
                {
                    var dishRepository = Resolve<IGenericRepository<Dish>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    foreach (var item in dto.ReservationDishes) 
                    { 
                        if(item.DishId != null)
                        {
                            if((await dishRepository!.GetByExpression(d => d.DishId == item.DishId && d.isAvailable, null)) == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy món ăn với id {item.DishId}");
                                return result;
                            }
                        } else
                        {
                            if ((await comboRepository!.GetByExpression(d => d.ComboId == item.ComboId && d.EndDate < utility.GetCurrentDateTimeInTimeZone(), null)) == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy combo với id {item.DishId}");
                                return result;
                            }
                        }

                        if(item.Quantity < 1)
                        {
                            result = BuildAppActionResultError(result, $"Số lượng phài lớn hơn 0");
                            return result;
                        }
                    }


                    var reservationRequest = _mapper.Map<ReservationRequest>(dto);
                    reservationRequest.StatusId = ReservationRequestStatus.PENDING;
                    reservationRequest.ReservationDishes = JsonConvert.SerializeObject(dto.ReservationDishes);  
                    await _repository.Insert(reservationRequest);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex) 
            { 
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllReservationRequest(ReservationRequestStatus? status, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                if (status.HasValue) 
                {
                    result.Result = await _repository.GetAllDataByExpression(null, pageNumber, pageSize, r => r.CreateDate, false, null);
                } else
                {
                    result.Result = await _repository.GetAllDataByExpression(r => r.StatusId == status, pageNumber, pageSize, r => r.CreateDate, false, null);
                }
            }
            catch (Exception ex) 
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateStatus(Guid reservationRequestId, ReservationRequestStatus status)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var reservationRequestDb = await _repository.GetById(reservationRequestId);
                if (reservationRequestDb == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy yêu cầu đặt bàn với id {reservationRequestId}");
                    return result;
                }

                if (reservationRequestDb.StatusId != ReservationRequestStatus.PENDING) 
                {
                    result = BuildAppActionResultError(result, $"Yêu cầu đã được xử lý trước đó");
                    return result;
                }

                reservationRequestDb.StatusId = status;
                await _repository.Update(reservationRequestDb);
                await _unitOfWork.SaveChangesAsync();   
            }
            catch (Exception ex) 
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
