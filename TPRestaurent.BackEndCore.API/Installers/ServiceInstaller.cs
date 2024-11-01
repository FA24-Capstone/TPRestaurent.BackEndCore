using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.Implementation;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Domain.Data;
using TPRestaurent.BackEndCore.Infrastructure.Implementation;

namespace TPRestaurent.BackEndCore.API.Installers;

public class ServiceInstaller : IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDBContext, TPRestaurentDBContext>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        //========//
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IDishService, DishService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IComboService, ComboService>();
        services.AddScoped<IMapService, MapService>();
        //services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IReservationRequestService, ReservationRequestService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IStoreCreditService, StoreCreditService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IFirebaseService, FirebaseService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<ITableSessionService, TableSessionService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IHashingService, HashingService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IDishManagementService, DishManagementService>();
        services.AddScoped<IOrderSessionService, OrderSessionService>();
        services.AddScoped<INotificationMessageService, NotificationMessageService>();
        services.AddScoped<IGroupedDishCraftService, GroupedDishCraftService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IDashboardService, DashboardService>();

    }
}