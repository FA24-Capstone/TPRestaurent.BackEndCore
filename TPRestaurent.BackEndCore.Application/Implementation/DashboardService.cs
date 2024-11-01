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

        public async Task<AppActionResult> GetOrderStatusReport()
        {
            var orderRepository = Resolve<IGenericRepository<Order>>();
            var result = new AppActionResult();
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();
            try
            {
                OrderStatusReportResponse orderStatusReportResponse = new OrderStatusReportResponse();
                var successfulOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Completed && p.OrderDate.Date == currentTime.Date, 0, 0, null, false, null);
                var cancellingOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Cancelled && p.OrderDate.Date == currentTime.Date, 0, 0, null, false, null);
                var pendingOrderDb = await orderRepository.GetAllDataByExpression(p => p.StatusId == OrderStatus.Pending && p.OrderDate.Date == currentTime.Date, 0, 0, null, false, null);

                orderStatusReportResponse.SuccessfullyOrderNumber = successfulOrderDb.Items.Count();
                orderStatusReportResponse.CancellingOrderNumber = cancellingOrderDb.Items.Count();
                orderStatusReportResponse.PendingOrderNumber = cancellingOrderDb.Items.Count();

                result.Result = orderStatusReportResponse;      
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetProfitReport()
        {
            var result = new AppActionResult();
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var transactionRepository = Resolve<IGenericRepository<Transaction>>();

            try
            {
                var todayStart = currentTime.Date;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);
                var yesterdayStart = todayStart.AddDays(-1);
                var yesterdayEnd = todayStart.AddTicks(-1);

                var presentTransactionDb = await transactionRepository!.GetAllDataByExpression(
                    p => p.Date >= todayStart &&
                         p.Date <= todayEnd &&
                         p.TransationStatusId == TransationStatus.SUCCESSFUL,
                    0, 0, null, false, null);

                var yesterdayTransactionDb = await transactionRepository!.GetAllDataByExpression(
                    p => p.Date >= yesterdayStart &&
                         p.Date <= yesterdayEnd &&
                         p.TransationStatusId == TransationStatus.SUCCESSFUL,
                    0, 0, null, false, null);

                var presentProfitNumber = presentTransactionDb.Items?.Sum(t => t.Amount) ?? 0;
                var yesterdayProfitNumber = yesterdayTransactionDb.Items?.Sum(t => t.Amount) ?? 0;

                var profitReportResponse = new ProfitReportResponse
                {
                    Profit = presentProfitNumber,
                    PercentProfit = yesterdayProfitNumber != 0
                        ? Math.Round((presentProfitNumber / yesterdayProfitNumber) * 100, 2)
                        : 0 
                };

                result.Result = profitReportResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetRevenueReportMonthly()
        {
            var result = new AppActionResult();
            var transactionRepository = Resolve<IGenericRepository<Transaction>>();
            var monthlyRevenue = new Dictionary<int, decimal>();

            try
            {
                for (int month = 1; month <= 12; month++)
                {
                    var transactions = await transactionRepository.GetAllDataByExpression(
                        p => p.Date.Month == month && p.TransationStatusId == TransationStatus.SUCCESSFUL,
                        0,
                        0,
                        null,
                        false,
                        null
                    );

                    var totalRevenue = transactions.Items.Sum(t => t.Amount); 
                    monthlyRevenue[month] = (decimal)totalRevenue;
                }

                result.Result = monthlyRevenue;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetTotalChef()
        {
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            var result = new AppActionResult();
            try
            {
                var chefDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_CHEF);
                if (chefDb == null)
                {
                    result.Result = 0;
                }
                else
                {
                    result.Result = chefDb.Count();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTotalCustomer()
        {
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var result = new AppActionResult();
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            CustomerStasticResponse customerStasticResponse = new CustomerStasticResponse();
            try
            {
                var roleDb = await roleRepository!.GetByExpression(p => p.Name == SD.RoleName.ROLE_CUSTOMER);
                var customerDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_CUSTOMER);
                var customerLastWeekDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_CUSTOMER);

                var customerLastWeek = customerLastWeekDb.Where(p => p.RegisteredDate.Date == currentTime.AddDays(-7).Date);
                var lastWeekCount = customerLastWeek.Count();
                var customerCount = customerDb.Count();

                customerStasticResponse.NumberOfCustomer = customerCount;
                customerStasticResponse.PercentIncrease = lastWeekCount == 0 ? 0 : Math.Round(
                    (double)customerCount / lastWeekCount
                );
                result.Result = customerStasticResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTotalDeliveringOrder()
        {
            var result = new AppActionResult();
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var orderRepository = Resolve<IGenericRepository<Order>>();
            try
            {
                var orderDb = await orderRepository!.GetAllDataByExpression(p => p.OrderDate.Day.Equals(currentTime.Day) && p.OrderTypeId == OrderType.Delivery, 0, 0, null, false, null);
                if (orderDb.Items.Count > 0 && orderDb.Items != null)
                {
                    result.Result = orderDb.Items!.Count();
                }
                else
                {
                    result.Result = 0;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTotalReservation()
        {
            var result = new AppActionResult();
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var orderRepository = Resolve<IGenericRepository<Order>>();
            try
            {
                var orderDb = await orderRepository!.GetAllDataByExpression(p => p.OrderDate.Date == currentTime.Date && p.OrderTypeId == OrderType.Reservation, 0, 0, null, false, null);
                if (orderDb.Items.Count > 0 && orderDb.Items != null)
                {
                    result.Result = orderDb.Items!.Count();
                }
                else
                {
                    result.Result = 0;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTotalShipper()
        {
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            var result = new AppActionResult();
            try
            {
                ShipperStatisticResponse shipperStatisticResponse = new ShipperStatisticResponse();
                var shipperDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_SHIPPER);
                shipperStatisticResponse.NumberOfShipperIsWorking = shipperDb.Where(p => p.IsDelivering).Count();
                shipperStatisticResponse.NumberOfShipper = shipperDb.Count;
                result.Result = shipperStatisticResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
