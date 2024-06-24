namespace TPRestaurent.BackEndCore.Application.IHubServices;

public interface IHubServices
{
    Task SendAsync(string method);
}