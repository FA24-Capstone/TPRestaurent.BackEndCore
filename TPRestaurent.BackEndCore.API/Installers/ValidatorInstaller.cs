namespace TPRestaurent.BackEndCore.API.Installers;

public class ValidatorInstaller : IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<HandleErrorValidator>();
        services.AddValidatorsFromAssemblyContaining<PrivateTourRequestDTO>();
    }
}