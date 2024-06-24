using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;

namespace TPRestaurent.BackEndCore.API.Middlewares;

public class RemoveCacheAtrribute : Attribute, IAsyncActionFilter
{
    private readonly string pathEndPoint;

    public RemoveCacheAtrribute(string pathEndPoint)
    {
        this.pathEndPoint = $"/{pathEndPoint}/";
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheConfiguration = context.HttpContext.RequestServices.GetRequiredService<RedisConfiguration>();
        if (!cacheConfiguration.Enabled)
        {
            await next();
            return;
        }
        //var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
        //var result = await next();
        //if (result.Result is ObjectResult okObjectResult)
        //{
        //    await cacheService.RemoveCacheResponseAsync(pathEndPoint);
        //}
    }
}