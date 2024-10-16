using Castle.DynamicProxy.Generators;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation;

public class NotificationMessageService : GenericBackendService, INotificationMessageService
{
    private IGenericRepository<NotificationMessage> _repository;
    public NotificationMessageService(IServiceProvider serviceProvider, IGenericRepository<NotificationMessage> repository) : base(serviceProvider)
    {
        _repository = repository;
    }

    public async Task<AppActionResult> GetNotificationMessageByAccountId(string accountId)
    {
        AppActionResult result = new AppActionResult();
        try
        {
            var notificationDb = await _repository.GetAllDataByExpression(n => n.AccountId.Equals(accountId), 0, 0, n => n.NotifyTime, false, null);
            result.Result = notificationDb;
        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, ex.Message);
        }
        return result;
    }

    public async Task<AppActionResult> GetNotificationMessageById(Guid notifiId)
    {
        var result = new AppActionResult();
        try
        {
            var notificationMessageDb = await _repository.GetByExpression(p => p.NotificationId == notifiId);
            if (notificationMessageDb == null)
            {
                return BuildAppActionResultError(result, $"Không tim thấy thông báo");
            }

            result.Result = notificationMessageDb;
        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, $"Có lỗi xảy ra khi sử dụng API với GoongMap {ex.Message} ");
        }
        return result;
    }
}