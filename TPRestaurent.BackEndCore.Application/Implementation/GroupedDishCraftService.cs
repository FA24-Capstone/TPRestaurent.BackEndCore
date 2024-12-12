using Newtonsoft.Json;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IHubServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class GroupedDishCraftService : GenericBackendService, IGroupedDishCraftService
    {
        private readonly IGenericRepository<GroupedDishCraft> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private IHubServices.IHubServices _hubServices;
        public GroupedDishCraftService(IGenericRepository<GroupedDishCraft> repository, IUnitOfWork unitOfWork, IServiceProvider provider, IHubServices.IHubServices hubServices) : base(provider)
        {
            this._repository = repository;
            _hubServices = hubServices;
            this._unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> GetAllGroupedDish()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var groupedDishDb = await _repository.GetAllDataByExpression(g => !g.IsFinished, 0, 0, g => g.GroupNumber, false, null);
                result.Result = groupedDishDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetGroupedDishById(Guid groupedDishId, Guid? dishId, bool? isMutual)
        {
            //tìm dish Id thuộc
            AppActionResult result = new AppActionResult();
            try
            {
                var groupedDishDb = await _repository.GetByExpression(g => g.GroupedDishCraftId == groupedDishId && !g.IsFinished, null);
                if (groupedDishDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy phiên gom món với id {groupedDishId}");
                }
                var groupedDishes = JsonConvert.DeserializeObject<KitchenGroupedDishResponse>(groupedDishDb.GroupedDishJson);
                if (dishId.HasValue)
                {
                    if (isMutual.HasValue)
                    {
                        if (isMutual.Value)
                        {
                            result.Result = groupedDishes.MutualOrderDishes.FirstOrDefault(m => m.Dish.DishId == dishId.Value);
                        }
                        else
                        {
                            result.Result = groupedDishes.SingleOrderDishes.FirstOrDefault(m => m.Dish.DishId == dishId.Value);
                        }
                    }
                }
                else
                {
                    result.Result = groupedDishes;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> InsertGroupedDish()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderSessionService = Resolve<IOrderSessionService>();
                var orderService = Resolve<IOrderService>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var hubService = Resolve<IHubServices.IHubServices>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                await orderService.NotifyReservationDishToKitchen();
                var groupedDishDb = await _repository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var openTimeConfig = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.OPEN_TIME), null);

                var previousTimeStamp = groupedDishDb.Items.OrderByDescending(g => g.EndTime).FirstOrDefault();

                DateTime?[] groupedTime = new DateTime?[2];
                groupedTime[0] = previousTimeStamp == null ? utility.GetCurrentDateInTimeZone().AddHours(double.Parse(openTimeConfig.CurrentValue)) : previousTimeStamp.EndTime;
                groupedTime[1] = currentTime;

                var groupedDishResult = await orderSessionService.GetGroupedDish(groupedTime);
                if (!groupedDishResult.IsSuccess)
                {
                    return BuildAppActionResultError(result, $"Lấy dữ liệu gom món tối ưu thất bại. Vui lòng thử lại.");
                }

                var groupedDishData = groupedDishResult.Result as KitchenGroupedDishResponse;
                if (groupedDishData == null || groupedDishData?.MutualOrderDishes?.Count == 0 && groupedDishData?.SingleOrderDishes?.Count == 0)
                {
                    return BuildAppActionResultError(result, $"Không có món mới.");
                }

                var newGroupDish = new GroupedDishCraft
                {
                    GroupedDishCraftId = Guid.NewGuid(),
                    GroupNumber = previousTimeStamp == null ? 1 : previousTimeStamp.GroupNumber + 1,
                    StartTime = previousTimeStamp == null ? utility.GetCurrentDateInTimeZone().AddHours(8) : previousTimeStamp.EndTime,
                    EndTime = currentTime,
                    IsFinished = false,
                    OrderDetailidList = string.Join(",", groupedDishData.OrderDetailIds),
                    GroupedDishJson = JsonConvert.SerializeObject(groupedDishData)
                };
                await _repository.Insert(newGroupDish);
                await _unitOfWork.SaveChangesAsync();
                await hubService.SendAsync(SD.SignalMessages.LOAD_ORDER);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateGroupedDish(List<Guid> orderDetailIds)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                List<GroupedDishCraft> groupedDishCrafts = new List<GroupedDishCraft>();
                foreach (var orderDetailId in orderDetailIds)
                {
                    var groupedDishDb = groupedDishCrafts.FirstOrDefault(g => g.OrderDetailidList.Contains(orderDetailId.ToString()));
                    if (groupedDishDb == null)
                    {
                        groupedDishDb = await _repository.GetByExpression(g => g.OrderDetailidList.Contains(orderDetailId.ToString()), null);
                        groupedDishCrafts.Add(groupedDishDb);
                    }
                    if (groupedDishDb == null)
                    {
                        return BuildAppActionResultError(result, $"Cập nhật thông tin gộp món thất bại");
                    }
                }

                foreach (var groupedDishCraft in groupedDishCrafts)
                {
                    if (!await UpdateGroupedDishJson(groupedDishCraft))
                    {
                        return BuildAppActionResultError(result, $"Cập nhật thông tin gộp món thất bại");
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<bool> UpdateGroupedDishJson(GroupedDishCraft groupedDishDb)
        {
            bool isSuccessful = false;
            try
            {
                var orderSessionService = Resolve<IOrderSessionService>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                DateTime?[] groupedTime = new DateTime?[2]
                {
                    groupedDishDb.StartTime,
                    groupedDishDb.EndTime
                };

                var groupedDishResult = await orderSessionService.GetGroupedDish(groupedTime);
                if (!groupedDishResult.IsSuccess)
                {
                    return isSuccessful;
                }

                var groupedDishData = groupedDishResult.Result as KitchenGroupedDishResponse;
                if (groupedDishData != null)
                {
                    CheckAndSetLateStatus(groupedDishData.SingleOrderDishes, groupedDishDb, currentTime);
                    CheckAndSetLateStatus(groupedDishData.MutualOrderDishes, groupedDishDb, currentTime);
                    groupedDishDb.GroupedDishJson = JsonConvert.SerializeObject(groupedDishData);
                    groupedDishDb.OrderDetailidList = string.Join(",", groupedDishData.OrderDetailIds);
                    if (groupedDishData.SingleOrderDishes.Count == 0 && groupedDishData.MutualOrderDishes.Count == 0)
                    {
                        groupedDishDb.IsFinished = true;
                    }
                }
                else
                {
                    groupedDishDb.GroupedDishJson = JsonConvert.SerializeObject(new KitchenGroupedDishResponse());
                    groupedDishDb.OrderDetailidList = "";
                    groupedDishDb.IsFinished = true;
                }
                isSuccessful = true;
            }
            catch (Exception ex)
            {
                isSuccessful = false;
            }
            return isSuccessful;
        }

        [Hangfire.Queue("update-late-warning-grouped-dish")]
        public async Task UpdateLateWarningGroupedDish()
        {
            try
            {
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var currentGroupedDishDb = await _repository.GetAllDataByExpression(g => !g.IsFinished, 0, 0, null, false, null);

                if (currentGroupedDishDb.Items.Count > 0)
                {
                    foreach (var groupedDish in currentGroupedDishDb.Items)
                    {
                        var groupDishFromJson = JsonConvert.DeserializeObject<KitchenGroupedDishResponse>(groupedDish.GroupedDishJson);

                        // Check late status for both SingleOrderDishes and MutualOrderDishes
                        CheckAndSetLateStatus(groupDishFromJson.SingleOrderDishes, groupedDish, currentTime);
                        CheckAndSetLateStatus(groupDishFromJson.MutualOrderDishes, groupedDish, currentTime);

                        groupedDish.GroupedDishJson = JsonConvert.SerializeObject(groupDishFromJson);
                    }

                    await _repository.UpdateRange(currentGroupedDishDb.Items);
                    await _unitOfWork.SaveChangesAsync();
                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER);


                }
            }
            catch (Exception ex)
            {
            }
        }

        private void CheckAndSetLateStatus(IEnumerable<KitchenGroupedDishItemResponse> orderDishes, GroupedDishCraft groupedDish, DateTime currentTime)
        {
            foreach (var groupedItem in orderDishes)
            {
                if (groupedItem.IsLate)
                    continue; // Skip if already marked as late

                if (IsLateConditionMet(groupedItem.UncheckedDishFromTableOrders, groupedDish, currentTime) ||
                    IsLateConditionMet(groupedItem.ProcessingDishFromTableOrders, groupedDish, currentTime))
                {
                    groupedItem.IsLate = true;
                }
            }
        }

        private bool IsLateConditionMet(IEnumerable<DishFromTableOrder> dishOrders, GroupedDishCraft groupedDish, DateTime currentTime)
        {
            foreach (var dish in dishOrders)
            {
                var preparationTime = dish.ComboOrderDetail?.PreparationTime ?? dish.OrderDetail?.PreparationTime;

                if (preparationTime.HasValue && groupedDish.EndTime.AddMinutes(preparationTime.Value) < currentTime)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task RemoveOverdueGroupedDish()
        {
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var groupedDishDb = await _repository.GetAllDataByExpression(null, 0, 0, null, false, null);
                if (groupedDishDb.Items.Count > 0)
                {
                    foreach (var groupedDish in groupedDishDb.Items)
                    {
                        if (groupedDish.StartTime.Date < currentTime.Date)
                        {
                            await _repository.DeleteById(groupedDish.GroupedDishCraftId);
                        }
                        else if (groupedDish.IsFinished)
                        {
                            await _repository.DeleteById(groupedDish.GroupedDishCraftId);
                        }
                        else if (string.IsNullOrEmpty(groupedDish.OrderDetailidList))
                        {
                            await _repository.DeleteById(groupedDish.GroupedDishCraftId);
                        }
                        else
                        {
                            List<Guid> orderDetailIds = JsonConvert.DeserializeObject<List<Guid>>(groupedDish.OrderDetailidList);
                            var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => orderDetailIds.Contains(o.OrderDetailId), 0, 0, null, false, o => o.OrderSession, o => o.Order);
                            if (orderDetailDb.Items.All(o => o.OrderDetailStatusId == OrderDetailStatus.Cancelled
                                                            || (o.Order.StatusId == OrderStatus.Cancelled)
                                                            || (o.OrderSessionId.HasValue && o.OrderSession.OrderSessionStatusId == OrderSessionStatus.Cancelled)))
                            {
                                await _repository.DeleteById(groupedDish.GroupedDishCraftId);
                            }
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<AppActionResult> UpdateForceGroupedDish(List<UpdateGroupedDishDto> dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var orderService = Resolve<IOrderService>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                List< UpdateOrderDetailItemRequest> updateRequest = new List< UpdateOrderDetailItemRequest>();
                foreach (var item in dto)
                {
                    var currentGroupedDishDb = await _repository.GetById(item.GroupedDishId);

                    if (currentGroupedDishDb != null)
                    {
                        var groupDishFromJson = JsonConvert.DeserializeObject<KitchenGroupedDishResponse>(currentGroupedDishDb.GroupedDishJson);

                        // Check late status for both SingleOrderDishes and MutualOrderDishes
                        var listToUpdate = groupDishFromJson.SingleOrderDishes.Where(d => d.Dish.DishId == item.DishId).ToList();
                        if (listToUpdate.Count == 0)
                        {
                            listToUpdate = groupDishFromJson.MutualOrderDishes.Where(d => d.Dish.DishId == item.DishId).ToList();
                        }


                        if (item.Size.HasValue)
                        {
                            foreach(var dish in listToUpdate)
                            {
                                if(dish.UncheckedDishFromTableOrders.Count > 0)
                                {
                                    dish.UncheckedDishFromTableOrders.Where(d => d.Quantity.DishSize.Id == item.Size.Value)
                                                                     .ToList()
                                                                     .ForEach(d => updateRequest.Add(new UpdateOrderDetailItemRequest
                                                                     {
                                                                         DishId = item.DishId,
                                                                         OrderDetailId = d.OrderDetail.OrderDetailId
                                                                     }));
                                } 
                                else if(dish.ProcessingDishFromTableOrders.Count > 0)
                                {
                                    dish.ProcessingDishFromTableOrders.Where(d => d.Quantity.DishSize.Id == item.Size.Value)
                                                                     .ToList()
                                                                     .ForEach(d => updateRequest.Add(new UpdateOrderDetailItemRequest
                                                                     {
                                                                         DishId = item.DishId,
                                                                         OrderDetailId = d.OrderDetail.OrderDetailId
                                                                     }));
                                }
                            }
                        }
                        else
                        {
                            foreach (var dish in listToUpdate)
                            {
                                if (dish.UncheckedDishFromTableOrders.Count > 0)
                                {
                                    dish.UncheckedDishFromTableOrders.ForEach(d => updateRequest.Add(new UpdateOrderDetailItemRequest
                                                                     {
                                                                         DishId = item.DishId,
                                                                         OrderDetailId = d.OrderDetail.OrderDetailId
                                                                     }));
                                }
                                if (dish.ProcessingDishFromTableOrders.Count > 0)
                                {
                                    dish.ProcessingDishFromTableOrders.ForEach(d => updateRequest.Add(new UpdateOrderDetailItemRequest
                                                                     {
                                                                         DishId = item.DishId,
                                                                         OrderDetailId = d.OrderDetail.OrderDetailId
                                                                     }));
                                }
                            }
                        }
                    }
                }
                result.Result = await orderService.UpdateOrderDetailStatus(updateRequest, true);
                await _hubServices.SendAsync(SD.SignalMessages.LOAD_USER_ORDER);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}