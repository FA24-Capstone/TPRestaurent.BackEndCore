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
            option.UseSqlServer(configuration["ConnectionStrings:Host"], options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 5, // Number of retry attempts
                    maxRetryDelay: TimeSpan.FromSeconds(30), // Delay between retries
                    errorNumbersToAdd: null // List of SQL error codes to include for retry
                );
            });
        });

        services.AddIdentity<Account, IdentityRole>().AddEntityFrameworkStores<TPRestaurentDBContext>()
            .AddDefaultTokenProviders();
    }
}
