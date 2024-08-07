using TPRestaurent.BackEndCore.Common.ConfigurationModel;

namespace TPRestaurent.BackEndCore.API.Installers;

public class MappingConfigurationInstaller : IInstaller
{
    public void InstallService(IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfiguration = new JWTConfiguration();
        configuration.GetSection("JWT").Bind(jwtConfiguration);
        services.AddSingleton(jwtConfiguration);

        var emailConfiguration = new EmailConfiguration();
        configuration.GetSection("Email").Bind(emailConfiguration);
        services.AddSingleton(emailConfiguration);

        var firebaseConfiguration = new FirebaseConfiguration();
        configuration.GetSection("Firebase").Bind(firebaseConfiguration);
        services.AddSingleton(firebaseConfiguration);

        var firebaseAdminSdkConfiguration = new FirebaseAdminSDK();
        configuration.GetSection("FirebaseAdminSDK").Bind(firebaseAdminSdkConfiguration);
        services.AddSingleton(firebaseAdminSdkConfiguration);

        var momoConfiguration = new MomoConfiguration();
        configuration.GetSection("Momo").Bind(momoConfiguration);
        services.AddSingleton(momoConfiguration);

        var vnPayConfiguration = new VnPayConfiguration();
        configuration.GetSection("Vnpay").Bind(vnPayConfiguration);
        services.AddSingleton(vnPayConfiguration);

        var smsConfiguration = new SmsConfiguration();
        configuration.GetSection("SmsConfiguration").Bind(smsConfiguration);
        services.AddSingleton(smsConfiguration);
    }
}