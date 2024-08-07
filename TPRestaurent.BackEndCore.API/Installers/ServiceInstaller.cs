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
        services.AddHttpClient();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        //========//
        services.AddScoped<IDBContext, TPRestaurentDBContext>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IDishService, DishService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IComboService, ComboService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IReservationRequestService, ReservationRequestService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IStoreCreditService, StoreCreditService>();
        services.AddScoped<ICustomerLovedDishService, CustomerLovedDishService>();
        services.AddScoped<ICustomerSavedCouponService, CustomerSavedCouponService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IFirebaseService, FirebaseService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IExcelService, ExcelService>();


    }
}