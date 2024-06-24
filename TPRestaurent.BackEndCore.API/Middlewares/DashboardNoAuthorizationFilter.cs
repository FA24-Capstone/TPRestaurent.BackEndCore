using Hangfire.Dashboard;

namespace TPRestaurent.BackEndCore.API.Middlewares;

public class DashboardNoAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}