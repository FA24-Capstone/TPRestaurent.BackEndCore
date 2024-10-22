using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class StoreCreditService : GenericBackendService, IStoreCreditService
    {
        private readonly IGenericRepository<Account> _repository;
        //private readonly IGenericRepository<StoreCreditHistory> _historyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public StoreCreditService(IGenericRepository<Account> repository,  IMapper mapper, IUnitOfWork unitOfWork, IServiceProvider service) : base(service)
        {
            _repository = repository;
            //_historyRepository = historyRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [Hangfire.Queue("change-over-due-store-credit")]
        public async Task ChangeOverdueStoreCredit()
        {
            AppActionResult result = new AppActionResult();
            var utility = Resolve<Utility>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var accountDb = await _repository.GetAllDataByExpression(p => p.ExpiredDate < currentTime, 0, 0, null, false, null);
                if (accountDb.Items!.Count > 0 && accountDb.Items != null)
                {
                    foreach (var account in accountDb!.Items!)
                    {
                        account.StoreCreditAmount = 0;
                        await _repository.Update(account);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            Task.CompletedTask.Wait();
        }

        public async Task<AppActionResult> AddStoreCredit(Guid transactionId)
        {
            AppActionResult result = new AppActionResult();
            using (var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    //Validate in transaction
                    var transactionRepository = Resolve<IGenericRepository<Transaction>>();
                    var transactionDb = await transactionRepository.GetByExpression(t => t.Id == transactionId && t.TransationStatusId == Domain.Enums.TransationStatus.SUCCESSFUL, t => t.Account);
                    if (transactionDb == null || string.IsNullOrEmpty(transactionDb.AccountId))
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy thông tin giao dịch cho việc nộp ví");
                    }

                    if (transactionDb.TransationStatusId == Domain.Enums.TransationStatus.APPLIED)
                    {
                        return BuildAppActionResultError(result, $"Giao dịch với id {transactionId} đã được cập nhật vào ví");
                    }

                    transactionDb.Account.StoreCreditAmount += transactionDb.Amount;

                    var utility = Resolve<Utility>();
                    var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                    var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                    if (configurationDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Xảy ra lỗi khi ghi lại thông tin nạp tiền. Vui lòng thử lại");
                    }
                    var expireTimeInDay = double.Parse(configurationDb.CurrentValue);

                    transactionDb.Account!.ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay);
                    transactionDb.TransationStatusId = Domain.Enums.TransationStatus.APPLIED;
                    await transactionRepository.Update(transactionDb);
                    await _repository.Update(transactionDb.Account!);
                    await _unitOfWork.SaveChangesAsync();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }
        }

        //public async Task<AppActionResult> GetStoreCreditByAccountId(string accountId)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        var storeCreditDb = await _repository.GetByExpression(s => s.AccountId == accountId, s => s.Account.Customer);
        //        if (storeCreditDb != null) 
        //        {
        //            var storeCreditHistoryDb = await _historyRepository.GetAllDataByExpression(s => s.StoreCreditId == storeCreditDb.StoreCreditId, 0, 0, s => s.Date, false, null);
        //            result.Result = new StoreCreditResponse
        //            {
        //                CustomerInfo = storeCreditDb.Account.Customer,
        //                StoreCredit = storeCreditDb,
        //                StoreCreditHistories = storeCreditHistoryDb.Items
        //            };
        //        }
        //    }
        //    catch (Exception ex) 
        //    { 
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}

        public Task<AppActionResult> RefundReservation(Guid reservationId)
        {
            throw new NotImplementedException();
        }
    }
}
