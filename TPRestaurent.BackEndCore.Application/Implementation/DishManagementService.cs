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

        public async Task UpdateComboAvailability()
        {
            try
            {
                var dishComboDb = await _dishComboRepository.GetAllDataByExpression(d => !d.IsDeleted && !d.ComboOptionSet.Combo.IsDeleted, 0, 0, null, false, o => o.DishSizeDetail, o => o.ComboOptionSet);
                if (dishComboDb.Items.Count > 0)
                {
                    foreach (var dishCombo in dishComboDb.Items)
                    {
                        if (dishCombo.DishSizeDetail.IsAvailable && dishCombo.Quantity <= dishCombo.DishSizeDetail.QuantityLeft)
                        {
                            dishCombo.IsAvailable = true;
                            dishCombo.QuantityLeft = dishCombo.DishSizeDetail.QuantityLeft / dishCombo.Quantity;
                        }
                    }
                    var comboDictionary = dishComboDb.Items.GroupBy(c => c.ComboOptionSet.ComboId)
                                                    .ToDictionary(c => c.Key, c => c.GroupBy(co => co.ComboOptionSetId)
                                                                                    .ToDictionary(co => co.Key, co => co.ToList()));
                    List<Combo> comboList = new List<Combo>();
                    foreach (var combo in comboDictionary)
                    {
                        var comboDb = await _comboRepository.GetById(combo.Key);
                        if (combo.Value.All(c => c.Value.Any(co => co.IsAvailable)))
                        {
                            comboDb.IsAvailable = true;
                        }
                        else
                        {
                            comboDb.IsAvailable = false;
                        }
                        comboList.Add(comboDb);
                    }
                    await _dishComboRepository.UpdateRange(dishComboDb.Items);
                    await _comboRepository.UpdateRange(comboList);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<AppActionResult> UpdateDishQuantity(List<UpdateDishQuantityRequest> dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                foreach (var item in dto)
                {
                    var dishSizeDetailDb = await _dishRepository.GetById(item.DishSizeDetailId);
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

                    if (dishSizeDetailDb.QuantityLeft <= 0)
                    {
                        dishSizeDetailDb.IsAvailable = false;
                    } else
                    {
                        dishSizeDetailDb.IsAvailable = true;
                    }

                    await _dishRepository.Update(dishSizeDetailDb);
                }
                if(!BuildAppActionResultIsError(result))
                {
                    await _unitOfWork.SaveChangesAsync();
                    await UpdateComboAvailability();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

    }
}
