using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;
using static TPRestaurent.BackEndCore.Common.Utils.SD;


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
                StatisticReportDashboardResponse statisticReportDashboardResponse = new StatisticReportDashboardResponse();
                var monthlyRevenue = new Dictionary<int, decimal>();

                if (monthlyRevenue.Count() > 0)
                {
                    monthlyRevenue.Clear();
                }

                for (int month = 1; month <= endDate.Value.Month; month++)
                {
                    var transactions = await transactionRepository!.GetAllDataByExpression(
                        p => p.Date.Month == month &&
                             p.Date.Year == endDate.Value.Year &&
                             p.TransationStatusId == TransationStatus.SUCCESSFUL,
                        0,
                        0,
                        null,
                        false,
                        null
                    );
                    var totalRevenue = transactions.Items.Sum(t => t.Amount);
                    monthlyRevenue[month] = (decimal)totalRevenue;
                }

                statisticReportDashboardResponse.MonthlyRevenue = monthlyRevenue;

                OrderStatusReportResponse orderStatusReportResponse = new OrderStatusReportResponse();
                var successfulOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Completed && p.OrderDate >= startDate && p.OrderDate <= endDate, 0, 0, null, false, null);
                var cancellingOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Cancelled && p.OrderDate >= startDate && p.OrderDate <= endDate, 0, 0, null, false, null);
                var pendingOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Pending && p.OrderDate >= startDate && p.OrderDate <= endDate, 0, 0, null, false, null);

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
            try
            {
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

                StatisticReportNumberResponse statisticReportNumberResponse = new StatisticReportNumberResponse();      

                ShipperStatisticResponse shipperStatisticResponse = new ShipperStatisticResponse();
                var shipperDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_SHIPPER);
                shipperStatisticResponse.NumberOfShipperIsWorking = shipperDb.Where(p => p.IsDelivering).Count();
                shipperStatisticResponse.NumberOfShipper = shipperDb.Count;

                statisticReportNumberResponse.ShipperStatisticResponse = shipperStatisticResponse;

                var reservationOrderDb = await orderRepository!.GetAllDataByExpression(
                                                                       p => p.OrderDate >= startDate && p.OrderDate <= endDate && p.OrderTypeId == OrderType.Reservation,
                                                                       0, 0, null, false, null);
                statisticReportNumberResponse.TotalReservationNumber = reservationOrderDb!.Items!.Count();

                var deliveringOrderDb = await orderRepository!.GetAllDataByExpression(
                                                                    p => p.OrderDate >= startDate && p.OrderDate <= endDate && p.OrderTypeId == OrderType.Delivery,
                                                                    0, 0, null, false, null);

                statisticReportNumberResponse.TotalDeliveringOrderNumber = deliveringOrderDb!.Items!.Count();

                CustomerStasticResponse customerStasticResponse = new CustomerStasticResponse();
                var customerDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_CUSTOMER);
                var customerLastWeek = customerDb.Where(p => p.RegisteredDate.Date >= startDate.Value.AddDays(-7).Date);
                var lastWeekCount = customerLastWeek.Count();
                var customerCount = customerDb.Count();

                customerStasticResponse.NumberOfCustomer = customerCount;
                customerStasticResponse.PercentIncrease = lastWeekCount == 0 ? 0 : Math.Round(
                    (double)customerCount / lastWeekCount
                );

                statisticReportNumberResponse.CustomerStasticResponse = customerStasticResponse;

                var chefDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_CHEF);
                statisticReportNumberResponse.TotalChefNumber = chefDb.Count();

                var profitReportResponse = new ProfitReportResponse();
                var todayStart = currentTime.Date;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);
                var yesterdayStart = startDate.Value.AddDays(-1);
                var yesterdayEnd = endDate.Value.AddTicks(-1);

                var presentTransactionDb = await transactionRepository!.GetAllDataByExpression(
                    p => p.Date >= startDate &&
                         p.Date <= endDate &&
                         p.TransationStatusId == TransationStatus.SUCCESSFUL,
                    0, 0, null, false, null);

                var yesterdayTransactionDb = await transactionRepository!.GetAllDataByExpression(
                    p => p.Date >= yesterdayStart &&
                         p.Date <= yesterdayEnd &&
                         p.TransationStatusId == TransationStatus.SUCCESSFUL,
                    0, 0, null, false, null);

                var presentProfitNumber = presentTransactionDb.Items?.Sum(t => t.Amount) ?? 0;
                var yesterdayProfitNumber = yesterdayTransactionDb.Items?.Sum(t => t.Amount) ?? 0;
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
