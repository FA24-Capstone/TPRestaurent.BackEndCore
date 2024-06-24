using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TPRestaurent.BackEndCore.API.Installers;

public class DbInstaller : IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TravelCapstoneDbContext>(option =>
        {
            option.UseSqlServer(configuration["ConnectionStrings:DBDocker"]);
        });

        services.AddIdentity<Account, IdentityRole>().AddEntityFrameworkStores<TravelCapstoneDbContext>()
            .AddDefaultTokenProviders();
    }
}