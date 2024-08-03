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
        //services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IDishService, DishService>();
        services.AddScoped<IComboService, ComboService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}