using Hangfire;
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
        private IOrderSessionService _orderSessionService;
        private IDishManagementService _dishManagementService;
        private IUnitOfWork _unitOfWork;
        private IStoreCreditService _storeCreditService;
        private IAccountService _accountService;
        private IGroupedDishCraftService _groupedDishCraftService;
        private ITransactionService _transactionService;
        private IInvoiceService _invoiceService;
        private IDishService _dishService;
        private ICouponService _couponService;
        private ITableService _tableService;

        public WorkerService(IServiceProvider serviceProvider,
            BackEndLogger logger,
            IUnitOfWork unitOfWork,
            IOrderService orderService,
            IOrderSessionService orderSessionService,
            IDishManagementService dishManagementService,
            IConfigService configService,
            IStoreCreditService storeCreditService,
            IAccountService accountService,
            IGroupedDishCraftService groupedDishCraftService,
            ITransactionService transactionService,
            IInvoiceService invoiceService,
            IDishService dishService,
            ICouponService couponService,
            ITableService tableService
            ) : base(serviceProvider)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            //_reservationService = reservationService;
            _configService = configService;
            _orderService = orderService;
            _orderSessionService = orderSessionService;
            _storeCreditService = storeCreditService;
            _accountService = accountService;
            _groupedDishCraftService = groupedDishCraftService;
            _transactionService = transactionService;
            _invoiceService = invoiceService;
            _dishService = dishService;
            _dishManagementService = dishManagementService;
            _couponService = couponService;
            _tableService = tableService;
        }

        public async Task Start()
        {
            double reloadGrouped = 0;
            try
            {
                var reloadGroupedDish = await _configService.GetByName(SD.DefaultValue.TIME_FOR_GROUPED_DISH);
                reloadGrouped = string.IsNullOrEmpty((reloadGroupedDish.Result as Configuration).CurrentValue) ? 10 : double.Parse((reloadGroupedDish.Result as Configuration).CurrentValue);
                TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                RecurringJob.AddOrUpdate(() => _orderService.NotifyReservationDishToKitchen(), Cron.MinuteInterval(2), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _orderService.AccountDailyReservationDish(), "01 0 * * *", vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _storeCreditService.ChangeOverdueStoreCredit(), Cron.DayInterval(1), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _accountService.DeleteOverdueOTP(), Cron.DayInterval(1), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _groupedDishCraftService.InsertGroupedDish(), Cron.MinuteInterval((int)Math.Round(reloadGrouped)), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _orderService.RemindOrderReservation(), Cron.MinuteInterval(30), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _transactionService.CancelPendingTransaction(), Cron.DayInterval(1), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _configService.ChangeConfigurationJob(), Cron.MinuteInterval(59), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _groupedDishCraftService.UpdateLateWarningGroupedDish(), Cron.MinuteInterval(5), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _orderSessionService.UpdateLateOrderSession(), Cron.MinuteInterval(5), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _orderService.CancelOrder(), Cron.MinuteInterval(10), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _invoiceService.GenerateInvoice(), "01 0 * * *", vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _dishService.AutoRefillDish(), "02 0 * * *", vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _groupedDishCraftService.RemoveOverdueGroupedDish(), "04 0 * * *", vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _dishManagementService.UpdateComboAvailability(), "03 0 * * *", vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _orderSessionService.ClearOrderSessionDaily(), Cron.DayInterval(1), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _couponService.GetBirthdayUserForCoupon(), Cron.MonthInterval(1), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _couponService.ResetUserRank(), Cron.MonthInterval(1), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _couponService.RemoveExpiredCoupon(), Cron.DayInterval(1), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _couponService.AssignCouponToUserWithRank(), "5 0 1 * *", vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _transactionService.LogMoneyInformationHacked(), Cron.HourInterval(4), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _couponService.UpgradeUserRank(), Cron.HourInterval(4), vietnamTimeZone);
                RecurringJob.AddOrUpdate(() => _tableService.UpdateTableAvailability(), Cron.MinuteInterval(5), vietnamTimeZone);

            }
            catch (Exception ex)
            {
            }

            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            RecurringJob.AddOrUpdate(() => _orderService.NotifyReservationDishToKitchen(), Cron.MinuteInterval(2), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderService.AccountDailyReservationDish(), "01 0 * * *", vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _storeCreditService.ChangeOverdueStoreCredit(), Cron.DayInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _accountService.DeleteOverdueOTP(), Cron.DayInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _groupedDishCraftService.InsertGroupedDish(), Cron.MinuteInterval((int)Math.Round(reloadGrouped)), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderService.RemindOrderReservation(), Cron.MinuteInterval(30), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _transactionService.CancelPendingTransaction(), Cron.DayInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _configService.ChangeConfigurationJob(), Cron.MinuteInterval(59), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _groupedDishCraftService.UpdateLateWarningGroupedDish(), Cron.MinuteInterval(5), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderSessionService.UpdateLateOrderSession(), Cron.MinuteInterval(5), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderService.CancelOrder(), Cron.MinuteInterval(10), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _invoiceService.GenerateInvoice(), "01 0 * * *", vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _dishService.AutoRefillDish(), "02 0 * * *", vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _groupedDishCraftService.RemoveOverdueGroupedDish(), "04 0 * * *", vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _dishManagementService.UpdateComboAvailability(), "03 0 * * *", vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _orderSessionService.ClearOrderSessionDaily(), Cron.DayInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _couponService.GetBirthdayUserForCoupon(), Cron.MonthInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _couponService.ResetUserRank(), Cron.MonthInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _couponService.RemoveExpiredCoupon(), Cron.DayInterval(1), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _couponService.AssignCouponToUserWithRank(), "5 0 1 * *", vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _transactionService.LogMoneyInformationHacked(), Cron.HourInterval(4), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _couponService.UpgradeUserRank(), Cron.HourInterval(4), vietnamTimeZone);
            RecurringJob.AddOrUpdate(() => _tableService.UpdateTableAvailability(), Cron.MinuteInterval(5), vietnamTimeZone);


            //RecurringJob.AddOrUpdate(() => _orderService.CancelOverReservation(), Cron.MinuteInterval(2), vietnamTimeZone);
            //RecurringJob.AddOrUpdate(() => _orderService.CancelReservation(), Cron.HourInterval(2), vietnamTimeZone);
        }
    }
}