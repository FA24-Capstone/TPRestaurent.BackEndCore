using Hangfire;
using Microsoft.EntityFrameworkCore;
using TPRestaurent.BackEndCore.API.Installers;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Implementation;
using TPRestaurent.BackEndCore.Domain.Data;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.InstallerServicesInAssembly(builder.Configuration);

var app = builder.Build();

app.UseSwagger(op => op.SerializeAsV2 = false);
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//app.UseMiddleware<TokenValidationMiddleware>();

app.MapControllers();
ApplyMigration();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new DashboardNoAuthorizationFilter() }
}); ;

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var workerService = serviceProvider.GetRequiredService<WorkerService>();
    await workerService.Start();
}
app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<TPRestaurentDBContext>();
        if (_db.Database.GetPendingMigrations().Count() > 0) _db.Database.Migrate();
    }
}