using Hangfire;
using Microsoft.EntityFrameworkCore;
using TPRestaurent.BackEndCore.API.Installers;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Implementation;
using TPRestaurent.BackEndCore.Domain.Data;
using TPRestaurent.BackEndCore.Infrastructure.ServerHub;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("*")
                          .AllowAnyMethod() // Allows GET, POST, PUT, DELETE, etc.
                          .AllowAnyHeader() // Allows all headers
                          //.AllowCredentials()
                          //.SetIsOriginAllowed(_ => true)
        );
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.InstallerServicesInAssembly(builder.Configuration);

//builder.Services.AddHangfireServer(options =>
//{
//    options.ServerName = string.Format("{0}:order", Environment.MachineName);
//    options.Queues = new[] { "update-order-status-before-meal-time" };
//    options.WorkerCount = 1;
//});

//builder.Services.AddHangfireServer(options =>
//{
//    options.ServerName = $"{Environment.MachineName}:order";
//    options.Queues = new[] { "account-daily-reservation-dish" };
//    options.WorkerCount = 1;
//});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:order";
    options.Queues = new[] { "notify-reservation-dish-to-kitchen" };
    options.WorkerCount = 1;
});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:order";
    options.Queues = new[] { "update-order-detail-status-before-dining" };
    options.WorkerCount = 1;
});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:store-credit";
    options.Queues = new[] { "change-over-due-store-credit" };
    options.WorkerCount = 1;
});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:account";
    options.Queues = new[] { "delete-overdue-otp" };
    options.WorkerCount = 1;
});

//builder.Services.AddHangfireServer(options =>
//{
//    options.ServerName = $"{Environment.MachineName}:order";
//    options.Queues = new[] { "cancel-reservation" };
//    options.WorkerCount = 1;
//});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:order";
    options.Queues = new[] { "cancel-order" };
    options.WorkerCount = 1;
});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:order";
    options.Queues = new[] { "remind-order-reservation" };
    options.WorkerCount = 1;
});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:transaction";
    options.Queues = new[] { "cancel-pending-transaction" };
    options.WorkerCount = 5;

});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:configuration";
    options.Queues = new[] { "change-configuration" };
    options.WorkerCount = 5;
});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:grouped-dish-craft";
    options.Queues = new[] { "update-late-warning-grouped-dish" };
    options.WorkerCount = 5;
});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:order-session";
    options.Queues = new[] { "update-late-order-session" };
    options.WorkerCount = 5;
});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:order";
    options.Queues = new[] { "cancel-delivery" };
    options.WorkerCount = 5;
});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:invoice";
    options.Queues = new[] { "generate-invoice" };
    options.WorkerCount = 5;
});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:dish";
    options.Queues = new[] { "auto-refill-dish" };
    options.WorkerCount = 5;
});
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:order-session";
    options.Queues = new[] { "clear-order-session-daily" };
    options.WorkerCount = 5;
});

var app = builder.Build();

app.UseSwagger(op => op.SerializeAsV2 = false);
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//app.UseMiddleware<TokenValidationMiddleware>();

app.MapControllers();
app.MapHub<NotificationHub>("/notifications");
//ApplyMigration();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new DashboardNoAuthorizationFilter() }
}); ;



//using (var scope = app.Services.CreateScope())
//{
//    var serviceProvider = scope.ServiceProvider;
//    var workerService = serviceProvider.GetRequiredService<WorkerService>();
//    await workerService.Start();
//}
app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<TPRestaurentDBContext>();
        if (_db.Database.GetPendingMigrations().Count() > 0) _db.Database.Migrate();
    }
}