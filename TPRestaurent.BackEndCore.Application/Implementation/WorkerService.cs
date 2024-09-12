using Hangfire;
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
        private IStoreCreditService _storeCreditService;    

        public WorkerService(IServiceProvider serviceProvider,
            BackEndLogger logger,
            IUnitOfWork unitOfWork,
            IOrderService orderService,
            ICustomerSavedCouponService customerSavedCouponService,
            //IReservationService reservationService,
            IConfigService configService,
            IStoreCreditService storeCreditService
            ) : base(serviceProvider)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            //_reservationService = reservationService;   
            _configService = configService;
            _orderService = orderService;
            _customerSavedCouponService = customerSavedCouponService;   
            _storeCreditService = storeCreditService;   
        }

        public async Task Start()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //RecurringJob.AddOrUpdate(() => _orderService.CancelOverReservation(), Cron.DayInterval(1), vietnamTimeZone);
            //RecurringJob.AddOrUpdate(() => _orderService.CancelOverReservation(), Cron.DayInterval(1), vietnamTimeZone);
            //RecurringJob.AddOrUpdate(() => _orderService.UpdateOrderDetailStatusBeforeDining(), Cron.DayInterval(1), vietnamTimeZone);
            //RecurringJob.AddOrUpdate(() => _customerSavedCouponService.UpdateExpiredCouponStatus(), Cron.DayInterval(1), vietnamTimeZone);
            //RecurringJob.AddOrUpdate(() => _storeCreditService.ChangeOverdueStoreCredit(), Cron.DayInterval(1), vietnamTimeZone);
        }
    }
}
