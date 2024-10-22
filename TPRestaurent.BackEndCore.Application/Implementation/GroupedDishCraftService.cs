﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
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
        public GroupedDishCraftService(IGenericRepository<GroupedDishCraft> repository, IUnitOfWork unitOfWork, IServiceProvider provider): base(provider) 
        {
            this._repository = repository;
            this._unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> GetAllGroupedDish()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var groupedDishDb = await _repository.GetAllDataByExpression(g => !g.IsFinished, 0, 0, null, false, null);
                result.Result = groupedDishDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetGroupedDishById(Guid groupedDishId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var groupedDishDb = await _repository.GetById(groupedDishId);
                if (groupedDishDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy phiên gom món với id {groupedDishId}");
                }
                result.Result = groupedDishDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task InsertGroupedDish()
        {
            try
            {
                var orderSessionService = Resolve<IOrderSessionService>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();

                var groupedDishDb = await _repository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var previousTimeStamp = groupedDishDb.Items.OrderByDescending(g => g.EndTime).First();

                DateTime?[] groupedTime = new DateTime?[2];
                groupedTime[0] = previousTimeStamp == null ? null : previousTimeStamp.EndTime;
                groupedTime[1] = currentTime;


                var groupedDishResult = await orderSessionService.GetGroupedDish(groupedTime);
                if (!groupedDishResult.IsSuccess)
                {
                    return;
                }

                var groupedDishData = groupedDishResult.Result as KitchenGroupedDishResponse;

                var newGroupDish = new GroupedDishCraft
                {
                    GroupedDishCraftId = Guid.NewGuid(),
                    GroupNumber = previousTimeStamp == null ? 1 : previousTimeStamp.GroupNumber + 1,
                    StartTime = previousTimeStamp.EndTime == null ? currentTime : currentTime,
                    EndTime = currentTime,
                    IsFinished = false,
                    OrderDetailidList = string.Join(",", groupedDishData.OrderDetailIds),
                    GroupedDishJson = JsonConvert.SerializeObject(groupedDishData)
                };
                await _repository.Insert(newGroupDish);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
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
                    if(!await UpdateGroupedDishJson(groupedDishCraft))
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

                groupedDishDb.GroupedDishJson = JsonConvert.SerializeObject(groupedDishData);

                if (groupedDishData.SingleOrderDishes.Count == 0 && groupedDishData.MutualOrderDishes.Count == 0)
                {
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
    }
}
