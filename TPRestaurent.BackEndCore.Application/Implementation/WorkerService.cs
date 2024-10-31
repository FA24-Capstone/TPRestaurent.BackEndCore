﻿using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class WorkerService : GenericBackendService
    {
        private BackEndLogger _logger;
        private IReservationService _reservationService;
        private IConfigService _configService; 
        private IOrderService _orderService; 
        private IUnitOfWork _unitOfWork;
        private IStoreCreditService _storeCreditService;  
        private IAccountService _accountService;
        private IGroupedDishCraftService _groupedDishCraftService;
        private ITransactionService _transactionService;

        public WorkerService(IServiceProvider serviceProvider,
            BackEndLogger logger,
            IUnitOfWork unitOfWork,
            IOrderService orderService,
            //IReservationService reservationService,
            IConfigService configService,
            IStoreCreditService storeCreditService,
            IAccountService accountService,
            IGroupedDishCraftService groupedDishCraftService, 
            ITransactionService transactionService
            ) : base(serviceProvider)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            //_reservationService = reservationService;   
            _configService = configService;
            _orderService = orderService;
            _storeCreditService = storeCreditService;   
            _accountService = accountService;   
            _groupedDishCraftService = groupedDishCraftService;
            _transactionService = transactionService;   
        }


        public async Task Start()
        {
            var reloadGroupedDish = await _configService.GetByName(SD.DefaultValue.TIME_FOR_GROUPED_DISH);
            var reloadGrouped = string.IsNullOrEmpty((reloadGroupedDish.Result as Configuration).CurrentValue) ? 10 : double.Parse((reloadGroupedDish.Result as Configuration).CurrentValue);
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            RecurringJob.AddOrUpdate(() => _orderService.UpdateOrderStatusBeforeMealTime(), Cron.HourInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderService.CancelOverReservation(), Cron.MinuteInterval(2), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderService.UpdateOrderDetailStatusBeforeDining(), Cron.HourInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _storeCreditService.ChangeOverdueStoreCredit(), Cron.DayInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _accountService.DeleteOverdueOTP(), Cron.DayInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderService.CancelReservation(), Cron.HourInterval(2), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _groupedDishCraftService.InsertGroupedDish(), Cron.MinuteInterval((int)Math.Round(reloadGrouped)), vietnamTimeZone);
            //RecurringJob.AddOrUpdate(() => _orderService.RemindOrderReservation(), Cron.MinuteInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _transactionService.CancelPendingTransaction(), Cron.DayInterval(1), vietnamTimeZone);
        }
    }
}
