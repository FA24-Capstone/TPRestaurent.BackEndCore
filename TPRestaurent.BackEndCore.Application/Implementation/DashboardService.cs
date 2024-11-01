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

        public Task<AppActionResult> GetOrderStatusReport(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public async Task<AppActionResult> GetProfitReport()
        {
            var result = new AppActionResult();
            var utility = Resolve<TPRestaurent.BackEndCore.Common.Utils.Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var transactionRepository = Resolve<IGenericRepository<Transaction>>();
            try
            {
                double presentProfitNumber = 0;
                double yesterdayProfitNumber = 0;
                ProfitReportResponse profitReportResponse = new ProfitReportResponse();
                var presentTransactionDb = await transactionRepository!.GetAllDataByExpression(p => p.Date.Date.Equals(currentTime.Date) && p.TransationStatusId == TransationStatus.SUCCESSFUL, 0, 0, null, false, null);
                if (presentTransactionDb.Items.Count > 0 && presentTransactionDb.Items != null)
                {
                    foreach (var presentTransaction in presentTransactionDb.Items)
                    {
                        presentProfitNumber += presentTransaction.Amount;
                    }
                }

                var yesterday = currentTime.AddDays(-1).Date;
                var yesterdayTransactionDb = await transactionRepository!.GetAllDataByExpression(
                    p => p.Date.Date == yesterday && p.TransationStatusId == TransationStatus.SUCCESSFUL,
                    0, 0, null, false, null);
                if (yesterdayTransactionDb.Items.Count > 0 && yesterdayTransactionDb.Items != null)
                {
                    foreach (var yesterdayTransaction in yesterdayTransactionDb.Items)
                    {
                        yesterdayProfitNumber += yesterdayTransaction.Amount;
                    }
                }

                profitReportResponse.Profit = presentProfitNumber;
                profitReportResponse.PercentProfit = Math.Round((presentProfitNumber / yesterdayProfitNumber));
                result.Result = profitReportResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public Task<AppActionResult> GetRevenueReport(DateTime dateTime)
        {
            throw new NotImplementedException();
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
                    result.Result = chefDb;
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

                var customerLastWeek = customerLastWeekDb.Where(p => p.RegisteredDate.Date == currentTime.AddDays(-7));
                customerStasticResponse.NumberOfCustomer = customerDb.Count();
                customerStasticResponse.PercentIncrease = Math.Round(
                customerStasticResponse.PercentIncrease = Math.Round(
                    (double)customerDb.Count() / customerLastWeek.Count()
                ));
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
                var shipperDb = await _userManager.GetUsersInRoleAsync(SD.RoleName.ROLE_SHIPPER);
                if (shipperDb == null)
                {
                    result.Result = 0;
                }
                else
                {
                    result.Result = shipperDb;
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
