using Microsoft.AspNetCore.SignalR;
using TPRestaurent.BackEndCore.Application.IHubServices;
using TPRestaurent.BackEndCore.Infrastructure.ServerHub;

namespace TPRestaurent.BackEndCore.Infrastructure.HubServices;

public class HubServices : IHubServices
{
    private readonly IHubContext<NotificationHub> _signalRHub;

    public HubServices(IHubContext<NotificationHub> signalRHub)
    {
        _signalRHub = signalRHub;
    }

    public async Task SendAsync(string method)
    {
        await _signalRHub.Clients.All.SendAsync(method);
    }
}