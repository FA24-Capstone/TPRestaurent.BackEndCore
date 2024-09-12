﻿using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class WorkerService : GenericBackendService
    {
        private BackEndLogger _logger;
        private IReservationService _reservationService;
        private IConfigService _configService; 
        private IOrderService _orderService; 
        private IUnitOfWork _unitOfWork;
        private ICustomerSavedCouponService _customerSavedCouponService;

        public WorkerService(IServiceProvider serviceProvider,
            BackEndLogger logger,
            IUnitOfWork unitOfWork,
            IOrderService orderService,
            ICustomerSavedCouponService customerSavedCouponService,
            //IReservationService reservationService,
            IConfigService configService
            ) : base(serviceProvider)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            //_reservationService = reservationService;   
            _configService = configService;
            _orderService = orderService;
            _customerSavedCouponService = customerSavedCouponService;   
        }

        public async Task Start()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //RecurringJob.AddOrUpdate(() =>  _reservationService.CancelOverdueReservations(), Cron.DayInterval(1), vietnamTimeZone);
            //RecurringJob.AddOrUpdate(() => _configService.ChangeConfigurationJob(), Cron.DayInterval(1), vietnamTimeZone);
            //BackgroundJob.Enqueue(() => _orderService.CancelOverReservation());
            //BackgroundJob.Enqueue(() => _orderService.CancelOverReservation());
            //BackgroundJob.Enqueue(() => _orderService.UpdateOrderDetailStatusBeforeDining());
            //BackgroundJob.Enqueue(() => _customerSavedCouponService.UpdateExpiredCouponStatus());
            BackgroundJob.Enqueue(() => _configService.ChangeConfigurationJob());
        }
    }
}
