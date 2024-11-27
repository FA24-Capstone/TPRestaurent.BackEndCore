namespace TPRestaurent.BackEndCore.API.Installers;

public class SignalRInstaller: IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR().AddAzureSignalR();
    }
}