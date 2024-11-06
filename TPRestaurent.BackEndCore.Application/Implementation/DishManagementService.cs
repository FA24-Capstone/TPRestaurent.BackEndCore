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
        private IGenericRepository<DishCombo> _dishComboRepository;
        private IGenericRepository<Combo> _comboRepository;
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;

        public DishManagementService(IServiceProvider serviceProvider, 
                                     IGenericRepository<DishSizeDetail> dishRepository, 
                                     IGenericRepository<DishCombo> dishComboRepository, 
                                     IGenericRepository<Combo> comboRepository,
                                     IMapper mapper, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _dishRepository = dishRepository;
            _dishComboRepository = dishComboRepository;
            _comboRepository = comboRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<double> CalculatePreparationTime(List<CalculatePreparationTime> dto)
        {
            double result = 0;
            try
            {
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                double preparationTime = 0;
               
                foreach (var preparationRequest in dto)
                {
                    preparationTime += preparationRequest.PreparationTime * preparationRequest.Quantity;
                }

                //AddConfig time
                result = preparationTime;
            }
            catch (Exception ex)
            {
                result = 0;
            }
            return result;
        }

        public async Task<AppActionResult> LoadDishRequireManualInput()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var dishRepository = Resolve<IGenericRepository<Dish>>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var dishDb = await _dishRepository.GetAllDataByExpression(d => !d.Dish.IsDeleted, 0, 0, null, false, null); 
                var comboDb = await _comboRepository.GetAllDataByExpression(d => !d.IsDeleted && d.StartDate <= currentTime && d.EndDate >= currentTime
                                                                                 , 0, 0, null, false, null);

                var dishReponse = new List<DishDetailRequireManualInputResponse>();
                var dishData = dishDb.Items.GroupBy(d => d.DishId).ToDictionary(d => d.Key, d => d.ToList());
                foreach (var dish in dishData)
                {
                    var dishDetail = new DishDetailRequireManualInputResponse();
                    dishDetail.Dish = await dishRepository.GetById(dish.Key);
                    dishDetail.DishSizeDetails = dish.Value;
                    dishReponse.Add(dishDetail);
                }
                result.Result = new DishRequireManualInputResponse
                {
                    Combos = comboDb.Items,
                    DishSizeDetails = dishReponse
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
                            if (!item.QuantityLeft.HasValue)
                            {
                                dishSizeDetailDb.QuantityLeft = item.DailyCountdown.Value;
                            }
                        }

                        if (dishSizeDetailDb.QuantityLeft == 0)
                        {
                            dishSizeDetailDb.IsAvailable = false;
                        }

                        await _dishRepository.Update(dishSizeDetailDb);
                    }
                    else if (item.DishComboId.HasValue)
                    {
                        var comboDb = await _dishComboRepository.GetById(item.DishComboId.Value);
                        if (item.QuantityLeft.HasValue)
                        {
                            comboDb.QuantityLeft = item.QuantityLeft.Value;
                        }

                        if (item.DailyCountdown.HasValue)
                        {
                            comboDb.DailyCountdown = item.DailyCountdown.Value;
                            if (!item.QuantityLeft.HasValue)
                            {
                                comboDb.QuantityLeft = item.DailyCountdown.Value;
                            }
                        }

                        if(comboDb.QuantityLeft == 0)
                        {
                            comboDb.IsAvailable = false;
                        }

                        await _dishComboRepository.Update(comboDb);
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
                var comboDb = await _dishComboRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
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
                await _dishComboRepository.UpdateRange(comboDb.Items);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<AppActionResult> UpdateDishQuantity(List<Guid> orderSessionIds)
        {
            AppActionResult result = new AppActionResult();
            try
            {

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
