using Hangfire;
using TPRestaurent.BackEndCore.Application.Implementation;

namespace TPRestaurent.BackEndCore.API.Installers;

public class HangfireInstaller : IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(x => x.UseSqlServerStorage(configuration["ConnectionStrings:DB"]));
        services.AddHangfireServer();
        services.AddScoped<WorkerService>();
    }
}