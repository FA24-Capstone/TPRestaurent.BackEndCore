﻿using Microsoft.AspNetCore.Identity;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class DashboardService : GenericBackendService, IDashboardService
    {
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly IGenericRepository<IdentityUserRole<string>> _userRoleRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Account> _userManager;

        public DashboardService(IServiceProvider serviceProvider, IGenericRepository<Account> accountRepository,
            UserManager<Account> userManager,
            IGenericRepository<IdentityUserRole<string>> userRoleRepository,
            RoleManager<IdentityRole> roleManager
            ) : base(serviceProvider)
        {
            _accountRepository = accountRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<AppActionResult> GetStatisticReportForDashboardReport(DateTime? startDate, DateTime? endDate)
        {
            var result = new AppActionResult();
            var transactionRepository = Resolve<IGenericRepository<Transaction>>();
            var orderRepository = Resolve<IGenericRepository<Order>>();
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            try
            {
                StatisticReportDashboardResponse statisticReportDashboardResponse = new StatisticReportDashboardResponse();
                var monthlyRevenue = new Dictionary<string, decimal>();

                if (monthlyRevenue.Count() > 0)
                {
                    monthlyRevenue.Clear();
                }

                var months = Enumerable.Range(0, ((endDate.Value.Year - startDate.Value.Year) * 12 + endDate.Value.Month - startDate.Value.Month + 1))
                               .Select(offset => new DateTime(startDate.Value.Year, startDate.Value.Month, 1).AddMonths(offset))
                               .ToList();

                foreach (var month in months)
                {
                    var profitDb = await orderRepository!.GetAllDataByExpression(
                          p => (p.OrderDate.Date.Month == month.Month && p.OrderDate.Year == month.Year
                                || p.MealTime.HasValue && p.MealTime.Value.Month == month.Month && p.MealTime.Value.Year == month.Year)
                                 && (p.OrderDate.Date > SD.MINIMUM_DATE
                                        && (!startDate.HasValue || startDate.HasValue && p.OrderDate.Date >= startDate.Value)
                                        && (!endDate.HasValue || endDate.HasValue && p.OrderDate.Date <= endDate.Value)
                                    || p.MealTime.HasValue
                                        && (!startDate.HasValue || startDate.HasValue && p.MealTime.Value.Date >= startDate.Value)
                                        && (!endDate.HasValue || endDate.HasValue && p.MealTime.Value.Date <= endDate.Value)
                                    )
                             && p.StatusId == OrderStatus.Completed,
                          0, 0, null, false, null);
                    var totalRevenue = profitDb.Items.Sum(t => t.TotalAmount);
                    monthlyRevenue[month.ToString("MM-yyyy")] = (decimal)totalRevenue;
                }

                statisticReportDashboardResponse.MonthlyRevenue = monthlyRevenue;

                OrderStatusReportResponse orderStatusReportResponse = new OrderStatusReportResponse();
                var successfulOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Completed
                                                                                          && (p.OrderDate.Date > SD.MINIMUM_DATE
                                                                                                && (!startDate.HasValue || startDate.HasValue && p.OrderDate.Date >= startDate.Value)
                                                                                                && (!endDate.HasValue || endDate.HasValue && p.OrderDate.Date <= endDate.Value)
                                                                                            || p.MealTime.HasValue
                                                                                                && (!startDate.HasValue || startDate.HasValue && p.MealTime.Value.Date >= startDate.Value)
                                                                                                && (!endDate.HasValue || endDate.HasValue && p.MealTime.Value.Date <= endDate.Value)
                                                                                            )
                                                                                          , 0, 0, null, false, null);
                var cancellingOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Cancelled
                                                                                          && (p.OrderDate.Date > SD.MINIMUM_DATE
                                                                                                && (!startDate.HasValue || startDate.HasValue && p.OrderDate.Date >= startDate.Value)
                                                                                                && (!endDate.HasValue || endDate.HasValue && p.OrderDate.Date <= endDate.Value)
                                                                                            || p.MealTime.HasValue
                                                                                                && (!startDate.HasValue || startDate.HasValue && p.MealTime.Value.Date >= startDate.Value)
                                                                                                && (!endDate.HasValue || endDate.HasValue && p.MealTime.Value.Date <= endDate.Value)
                                                                                            )
                                                                                          , 0, 0, null, false, null);
                var pendingOrderDb = await orderRepository.GetAllDataByExpression(p => (p.StatusId != OrderStatus.Cancelled && p.StatusId != OrderStatus.Completed)
                                                                                          && (p.OrderDate.Date > SD.MINIMUM_DATE
                                                                                                && (!startDate.HasValue || startDate.HasValue && p.OrderDate.Date >= startDate.Value)
                                                                                                && (!endDate.HasValue || endDate.HasValue && p.OrderDate.Date <= endDate.Value)
                                                                                            || p.MealTime.HasValue
                                                                                                && (!startDate.HasValue || startDate.HasValue && p.MealTime.Value.Date >= startDate.Value)
                                                                                                && (!endDate.HasValue || endDate.HasValue && p.MealTime.Value.Date <= endDate.Value)
                                                                                            )
                                                                                          , 0, 0, null, false, null);

                orderStatusReportResponse.SuccessfullyOrderNumber = successfulOrderDb.Items.Count();
                orderStatusReportResponse.CancellingOrderNumber = cancellingOrderDb.Items.Count();
                orderStatusReportResponse.PendingOrderNumber = cancellingOrderDb.Items.Count();

                statisticReportDashboardResponse.OrderStatusReportResponse = orderStatusReportResponse;

                result.Result = statisticReportDashboardResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetStatisticReportForNumberReport(DateTime? startDate, DateTime? endDate)
        {
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var orderRepository = Resolve<IGenericRepository<Order>>();
            var result = new AppActionResult();
            var transactionRepository = Resolve<IGenericRepository<Transaction>>();
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            if (startDate.Value == default(DateTime))
            {
                startDate = TimeZoneInfo.ConvertTime(
                    new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0),
                    vietnamTimeZone);
            }
            if (endDate.Value == default(DateTime))
            {
                endDate = TimeZoneInfo.ConvertTime(
                    new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 23, 59, 59),
                    vietnamTimeZone);
            }
            try
            {
                StatisticReportNumberResponse statisticReportNumberResponse = new StatisticReportNumberResponse();

                ShipperStatisticResponse shipperStatisticResponse = new ShipperStatisticResponse();
                var shipperDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_SHIPPER);
                var shipperList = shipperDb.Where(p => p.RegisteredDate >= startDate.Value && p.RegisteredDate <= endDate.Value);
                shipperStatisticResponse.NumberOfShipperIsWorking = shipperList.Where(p => p.IsDelivering).Count();
                shipperStatisticResponse.NumberOfShipper = shipperList.Count();

                statisticReportNumberResponse.ShipperStatisticResponse = shipperStatisticResponse;

                var reservationOrderDb = await orderRepository!.GetAllDataByExpression(p =>
                                                                       p.OrderTypeId == OrderType.Reservation
                                                                       && p.MealTime.HasValue
                                                                       && (!startDate.HasValue || startDate.HasValue && p.MealTime.Value.Date >= startDate.Value)
                                                                       && (!endDate.HasValue || endDate.HasValue && p.MealTime.Value.Date <= endDate.Value),
                                                                       0, 0, null, false, null);
                statisticReportNumberResponse.TotalReservationNumber = reservationOrderDb!.Items!.Count();

                var deliveringOrderDb = await orderRepository!.GetAllDataByExpression(p => p.OrderTypeId == OrderType.Delivery
                                                                                        && p.OrderDate.Date > SD.MINIMUM_DATE
                                                                                        && (!startDate.HasValue || startDate.HasValue && p.OrderDate.Date >= startDate.Value)
                                                                                        && (!endDate.HasValue || endDate.HasValue && p.OrderDate.Date <= endDate.Value),
                                                                    0, 0, null, false, null);

                statisticReportNumberResponse.TotalDeliveringOrderNumber = deliveringOrderDb!.Items!.Count();

                CustomerStasticResponse customerStasticResponse = new CustomerStasticResponse();
                var customerDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_CUSTOMER);
                var customerList = customerDb.Where(p => p.RegisteredDate >= startDate.Value && p.RegisteredDate <= endDate.Value);
                var customerLastWeek = customerDb.Where(p => p.RegisteredDate.Date >= startDate.Value.AddDays(-7).Date);
                var lastWeekCount = customerLastWeek.Count();
                var customerCount = customerList.Count();

                customerStasticResponse.NumberOfCustomer = customerCount;
                customerStasticResponse.PercentIncrease = lastWeekCount == 0 ? 0 : Math.Round(
                    (double)customerCount / lastWeekCount
                );

                statisticReportNumberResponse.CustomerStasticResponse = customerStasticResponse;

                var chefDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_CHEF);
                var chefList = chefDb.Where(p => p.RegisteredDate >= startDate.Value && p.RegisteredDate <= endDate.Value);
                statisticReportNumberResponse.TotalChefNumber = chefList.Count();

                var profitReportResponse = new ProfitReportResponse();
                var todayStart = currentTime.Date;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);
                var yesterdayStart = startDate.Value.AddDays(-1);
                var yesterdayEnd = endDate.Value.AddTicks(-1);

                var presentProfitDb = await orderRepository!.GetAllDataByExpression(
                    p => (p.OrderDate.Date > SD.MINIMUM_DATE
                            && (!startDate.HasValue || startDate.HasValue && p.OrderDate.Date >= startDate.Value)
                            && (!endDate.HasValue || endDate.HasValue && p.OrderDate.Date <= endDate.Value)
                          || p.MealTime.HasValue
                            && (!startDate.HasValue || startDate.HasValue && p.MealTime.Value.Date >= startDate.Value)
                            && (!endDate.HasValue || endDate.HasValue && p.MealTime.Value.Date <= endDate.Value))
                         && p.StatusId == OrderStatus.Completed,
                    0, 0, null, false, null);

                var yesterdayProfitDb = await orderRepository!.GetAllDataByExpression(
                    p => p.OrderDate.Date > SD.MINIMUM_DATE
                            && p.OrderDate.Date >= yesterdayStart
                            && p.OrderDate.Date <= yesterdayEnd
                          || p.MealTime.HasValue
                            && p.MealTime.Value.Date >= yesterdayStart
                            && p.MealTime.Value.Date <= yesterdayEnd
                         && p.StatusId == OrderStatus.Completed,
                    0, 0, null, false, null);

                var presentProfitNumber = presentProfitDb.Items?.Sum(t => t.TotalAmount) ?? 0;
                var yesterdayProfitNumber = yesterdayProfitDb.Items?.Sum(t => t.TotalAmount) ?? 0;
                var percentProfit = yesterdayProfitNumber != 0
                        ? Math.Round((presentProfitNumber / yesterdayProfitNumber), 2)
                        : 0;
                if (presentProfitNumber < yesterdayProfitNumber)
                {
                    profitReportResponse.PercentProfitCompareToYesterday = -percentProfit;
                }
                else
                {
                    profitReportResponse.PercentProfitCompareToYesterday = percentProfit;
                }

                profitReportResponse.Profit = presentProfitNumber;

                statisticReportNumberResponse.ProfitReportResponse = profitReportResponse;

                result.Result = statisticReportNumberResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}