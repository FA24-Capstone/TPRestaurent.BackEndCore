using TPRestaurent.BackEndCore.Application.IHubServices;
using TPRestaurent.BackEndCore.Infrastructure.HubServices;

namespace TPRestaurent.BackEndCore.API.Installers;

public class HubInstaller : IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddSingleton<IHubServices, HubServices>();
        
    }
}