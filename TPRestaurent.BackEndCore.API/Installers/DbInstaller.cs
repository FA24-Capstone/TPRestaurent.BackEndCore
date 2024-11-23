using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPRestaurent.BackEndCore.Domain.Data;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.API.Installers;

public class DbInstaller : IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TPRestaurentDBContext>(option =>
        {
            option.UseSqlServer(configuration["ConnectionStrings:Host"],
                options => options.EnableRetryOnFailure(
                    maxRetryCount: 5, // Number of retries
                    maxRetryDelay: TimeSpan.FromSeconds(5), // Delay between retries
                    errorNumbersToAdd: null // Optionally, specify error numbers to trigger retries
                )
                );
        });

        services.AddIdentity<Account, IdentityRole>().AddEntityFrameworkStores<TPRestaurentDBContext>()
            .AddDefaultTokenProviders();
    }
}