using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class DishManagementService : GenericBackendService, IDishManagementService
    {
        private IGenericRepository<DishSizeDetail> _dishRepository;
        private IGenericRepository<Combo> _comboRepository;
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;

        public DishManagementService(IServiceProvider serviceProvider, IGenericRepository<DishSizeDetail> dishRepository, IGenericRepository<Combo> comboRepository, IMapper mapper, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _dishRepository = dishRepository;
            _comboRepository = comboRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> LoadDishRequireManualInput()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var dishDb = await _dishRepository.GetAllDataByExpression(d => d.IsAvailable && d.QuantityLeft.HasValue && d.QuantityLeft.Value == -1, 0, 0, null, false, null); 
                var comboDb = await _comboRepository.GetAllDataByExpression(d => d.StartDate <= currentTime && d.EndDate >= currentTime 
                                                                                 && d.QuantityLeft.HasValue && d.QuantityLeft.Value == -1, 0, 0, null, false, null);
                result.Result = new DishRequireManualInputResponse
                {
                    Combos = comboDb.Items,
                    DishSizeDetails = dishDb.Items
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateDishQuantity(List<UpdateDishQuantityRequest> dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                foreach (var item in dto)
                {
                    if (item.DishSizeDetailId.HasValue)
                    {
                        var dishSizeDetailDb = await _dishRepository.GetById(item.DishSizeDetailId.Value);
                        if (item.QuantityLeft.HasValue)
                        {
                            dishSizeDetailDb.QuantityLeft = item.QuantityLeft.Value;
                        }

                        if (item.DailyCountdown.HasValue)
                        {
                            dishSizeDetailDb.DailyCountdown = item.DailyCountdown.Value;
                        }

                        if (dishSizeDetailDb.QuantityLeft == 0)
                        {
                            dishSizeDetailDb.IsAvailable = false;
                        }

                        await _dishRepository.Update(dishSizeDetailDb);
                    }
                    else if (item.ComboId.HasValue)
                    {
                        var comboDb = await _comboRepository.GetById(item.ComboId.Value);
                        if (item.QuantityLeft.HasValue)
                        {
                            comboDb.QuantityLeft = item.QuantityLeft.Value;
                        }

                        if (item.DailyCountdown.HasValue)
                        {
                            comboDb.DailyCountdown = item.DailyCountdown.Value;
                        }

                        if(comboDb.QuantityLeft == 0)
                        {
                            comboDb.IsAvailable = false;
                        }

                        await _comboRepository.Update(comboDb);
                    }
                }
                if(!BuildAppActionResultIsError(result))
                {
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task UpdateDishQuantity()
        {
            try
            {
                var dishSizeDetailDb = await _dishRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var comboDb = await _comboRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                foreach (var dish in dishSizeDetailDb.Items)
                {
                    if(dish.DailyCountdown < 0)
                    {
                        dish.QuantityLeft = null;
                    }else if(dish.DailyCountdown == 0)
                    {
                        dish.QuantityLeft = 0;
                        if (dish.IsAvailable)
                        {
                            dish.IsAvailable = false;
                        }
                    } else
                    {
                        dish.QuantityLeft = dish.DailyCountdown;
                        if (!dish.IsAvailable)
                        {
                            dish.IsAvailable = true;
                        }
                    }
                }

                foreach (var combo in comboDb.Items)
                {
                    if (combo.DailyCountdown < 0)
                    {
                        combo.QuantityLeft = null;
                    }
                    else if (combo.DailyCountdown == 0)
                    {
                        combo.QuantityLeft = 0;
                        if (combo.IsAvailable)
                        {
                            combo.IsAvailable = false;
                        }
                    }
                    else
                    {
                        combo.QuantityLeft = combo.DailyCountdown;
                        if (!combo.IsAvailable)
                        {
                            combo.IsAvailable = true;
                        }
                    }
                }

                await _dishRepository.UpdateRange(dishSizeDetailDb.Items);
                await _comboRepository.UpdateRange(comboDb.Items);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
