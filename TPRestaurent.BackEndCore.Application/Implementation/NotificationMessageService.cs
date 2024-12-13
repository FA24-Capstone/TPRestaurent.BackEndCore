using Microsoft.AspNetCore.Identity;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;
using Utility = TPRestaurent.BackEndCore.Common.Utils.Utility;

namespace TPRestaurent.BackEndCore.Application.Implementation;

public class NotificationMessageService : GenericBackendService, INotificationMessageService
{
    private IGenericRepository<NotificationMessage> _repository;
    private IUnitOfWork _unitOfWork;
    public NotificationMessageService(IServiceProvider serviceProvider, IGenericRepository<NotificationMessage> repository, IUnitOfWork unitOfWork) : base(serviceProvider)
    {
        _unitOfWork = unitOfWork;
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

    public async Task<AppActionResult> GetNotificationMessageById(Guid notificationId)
    {
        var result = new AppActionResult();
        try
        {
            var notificationMessageDb = await _repository.GetByExpression(p => p.NotificationId == notificationId);
            if (notificationMessageDb == null)
            {
                throw new Exception($"Không tim thấy thông báo");
            }

            result.Result = notificationMessageDb;
        }
        catch (Exception ex)
        {
            return BuildAppActionResultError(result, $"Có lỗi xảy ra khi sử dụng API với GoongMap {ex.Message} ");
        }
        return result;
    }

    public async Task<AppActionResult> MarkAllMessageAsRead(string accountId)
    {
        var result = new AppActionResult();
        try
        {
            var notificationMessageDb = await _repository.GetAllDataByExpression(p => !p.IsRead && p.AccountId.Equals(accountId), 0, 0, null, false, null);
            if (notificationMessageDb.Items.Count > 0)
            {
                notificationMessageDb.Items.ForEach(n => n.IsRead = true);
                await _repository.UpdateRange(notificationMessageDb.Items);
                await _unitOfWork.SaveChangesAsync();
            }

            result.Result = notificationMessageDb;
        }
        catch (Exception ex)
        {
            return BuildAppActionResultError(result, $"Có lỗi xảy ra khi sử dụng API với GoongMap {ex.Message} ");
        }
        return result;
    }

    public async Task<AppActionResult> MarkMessageAsRead(Guid notificationId)
    {
        var result = new AppActionResult();
        try
        {
            var notificationDb = await _repository.GetByExpression(n => n.NotificationId == notificationId && !n.IsRead, null);
            if (notificationDb == null)
            {
                throw new Exception($"Không tìm thấy thông báo chưa đọc với id {notificationId}");
            }
            if (!notificationDb.IsRead)
            {
                notificationDb.IsRead = true;
                await _repository.Update(notificationDb);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, ex.Message);
        }
        return result;
    }

    public async Task<AppActionResult> SendNotificationToAccountAsync(string accountId, string message, bool ignoreSaveChanges = false)
    {
        var result = new AppActionResult();
        try
        {
            var utility = Resolve<Utility>();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            var tokenRepository = Resolve<IGenericRepository<Token>>();
            var fireBaseService = Resolve<IFirebaseService>();
            var notificationMessageRepository = Resolve<IGenericRepository<NotificationMessage>>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();
            if (string.IsNullOrEmpty(message))
            {

            }

            var accountDb = await accountRepository!.GetById(accountId);
            if (accountDb == null)
            {
                throw new Exception($"Không tìm thấy tài khoản với id {accountId}");
            }

            var tokenDb = await tokenRepository!.GetAllDataByExpression(p => p.AccountId == accountId, 0, 0, null, false, p => p.Account);
            if (tokenDb.Items!.Count > 0 && tokenDb.Items != null)
            {
                var deviceTokenList = tokenDb.Items.Where(p => !string.IsNullOrEmpty(p.DeviceToken)).Select(p => p.DeviceToken);
                if (deviceTokenList != null)
                {
                    

                    var notification = new NotificationMessage
                    {
                        NotificationId = Guid.NewGuid(),
                        NotificationName = "Bạn có thông báo mới",
                        Messages = message,
                        NotifyTime = currentTime,
                        AccountId = accountDb.Id,
                    };

                    await notificationMessageRepository!.Insert(notification);
                    if (deviceTokenList.Count() > 0)
                    {
                        await fireBaseService!.SendMulticastAsync(deviceTokenList.ToList(), "Nhà hàng có một thông báo mới", message, result);
                    }
                }
            }
            if (!BuildAppActionResultIsError(result) && !ignoreSaveChanges)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, ex.Message);
        }
        return result;
    }

    public async Task<AppActionResult> SendNotificationToRoleAsync(string roleName, string message)
    {
        var result = new AppActionResult();
        var utility = Resolve<Utility>();
        var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
        var fireBaseService = Resolve<IFirebaseService>();
        var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
        var tokenRepository = Resolve<IGenericRepository<Token>>();
        var notificationMessageRepository = Resolve<IGenericRepository<NotificationMessage>>();
        var tokenList = new List<string>();
        try
        {
            var roleDb = await roleRepository!.GetByExpression(p => p.Name == roleName);
            if (roleDb == null)
            {
                throw new Exception($"Không tìm thấy vai trò {roleName}");
            }
            var userRoleDb = await userRoleRepository!.GetAllDataByExpression(p => p.RoleId == roleDb!.Id, 0, 0, null, false, null);
            if (userRoleDb == null)
            {
                throw new Exception($"Không tìm thấy danh sách user với role {roleDb.Id}");
            }
            foreach (var user in userRoleDb!.Items!)
            {
                var tokenDb = await tokenRepository!.GetAllDataByExpression(p => p.AccountId == user.UserId, 0, 0, null, false, p => p.Account);
                foreach (var token in tokenDb.Items)
                {
                    if (!string.IsNullOrEmpty(token.DeviceToken))
                    {
                        tokenList.Add(token.DeviceToken);
                    }
                }

                var notificationList = new List<NotificationMessage>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var notification = new NotificationMessage
                {
                    NotificationId = Guid.NewGuid(),
                    NotificationName = "Nhà hàng có thông báo mới",
                    Messages = message,
                    NotifyTime = currentTime,
                    AccountId = user.UserId,
                };
                notificationList.Add(notification);

                await notificationMessageRepository!.InsertRange(notificationList);
                if (tokenList.Count() > 0)
                {
                    await fireBaseService!.SendMulticastAsync(tokenList, "Nhà hàng có một thông báo mới", message, result);
                }
            }

            if (!BuildAppActionResultIsError(result))
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, ex.Message);
        }
        return result;
    }

    public async Task<AppActionResult> SendNotificationToShipperAsync(string accountId, string message)
    {
        var result = new AppActionResult();

        try
        {
            var utility = Resolve<Utility>();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            var tokenRepository = Resolve<IGenericRepository<Token>>();
            var fireBaseService = Resolve<IFirebaseService>();
            var notificationMessageRepository = Resolve<IGenericRepository<NotificationMessage>>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();

            var accountDb = await accountRepository!.GetById(accountId);
            if (accountDb == null)
            {
                throw new Exception($"Không tìm thấy tài khoản với id {accountId}");
            }

            var tokenDb = await tokenRepository!.GetAllDataByExpression(p => p.AccountId == accountId, 0, 0, null,
                false, p => p.Account);
            if (tokenDb.Items!.Count > 0 && tokenDb.Items != null)
            {
                var deviceTokenList = tokenDb.Items.Where(p => !string.IsNullOrEmpty(p.DeviceToken))
                    .Select(p => p.DeviceToken).ToList();
                if (deviceTokenList != null)
                {

                    var notification = new NotificationMessage
                    {
                        NotificationId = Guid.NewGuid(),
                        NotificationName = "Bạn có thông báo mới từ nhà hàng",
                        Messages = message,
                        NotifyTime = currentTime,
                        AccountId = accountDb.Id,
                    };

                    await notificationMessageRepository!.Insert(notification);
                    if (deviceTokenList.Count() > 0)
                    {
                        await fireBaseService!.SendMulticastAsync(deviceTokenList.ToList(),
                            "Bạn có thông báo mới từ nhà hàng", message, result);
                    }
                }
            }

            if (!BuildAppActionResultIsError(result))
            {
                await _unitOfWork.SaveChangesAsync();
            }

        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, ex.Message);
        }
        return result;
    }
}