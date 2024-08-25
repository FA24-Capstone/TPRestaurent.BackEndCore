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
        private readonly IGenericRepository<StoreCredit> _repository;
        private readonly IGenericRepository<StoreCreditHistory> _historyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public StoreCreditService(IGenericRepository<StoreCredit> repository, IGenericRepository<StoreCreditHistory> historyRepository, IMapper mapper, IUnitOfWork unitOfWork, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _historyRepository = historyRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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
                    var transactionDb = await transactionRepository.GetById(transactionId);
                    if (!transactionDb.StoreCreditId.HasValue)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy thông tin giao dịch cho việc nộp ví");
                    }
                    var storeCreditDb = await _repository.GetById(transactionDb.StoreCreditId);
                    if (storeCreditDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy thông tin ví với id {transactionDb.StoreCreditId.Value}");
                    }

                    var appliedTransactionDb = await _historyRepository.GetAllDataByExpression(h =>  h.TransactionId == transactionId,0,0, null, false, null);
                    if (appliedTransactionDb.Items.Count > 0)
                    {
                        return BuildAppActionResultError(result, $"Giao dịch với id {transactionId} đã được áp dụng");
                    }

                    storeCreditDb.Amount += transactionDb.Amount;

                    var utility = Resolve<Utility>();
                    var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                    var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                    if (configurationDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Xảy ra lỗi khi ghi lại thông tin nạp tiền. Vui lòng thử lại");
                    }
                    var expireTimeInDay = double.Parse(configurationDb.PreValue);

                    storeCreditDb.ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay);

                    var storeCreditHistory = new StoreCreditHistory
                    {
                        StoreCreditHistoryId = Guid.NewGuid(),
                        StoreCreditId = storeCreditDb.StoreCreditId,
                        Date = utility.GetCurrentDateInTimeZone(),
                        Amount = transactionDb.Amount,
                        IsInput = true,
                        TransactionId = transactionId,
                    };

                    await _repository.Update(storeCreditDb);
                    await _historyRepository.Insert(storeCreditHistory);
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

        public async Task<AppActionResult> GetStoreCreditByAccountId(string accountId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var storeCreditDb = await _repository.GetByExpression(s => s.AccountId == accountId, null);
                if (storeCreditDb != null) 
                {
                    var storeCreditHistoryDb = await _historyRepository.GetAllDataByExpression(s => s.StoreCreditId == storeCreditDb.StoreCreditId, 0, 0, s => s.Date, false, null);
                    result.Result = new StoreCreditResponse
                    {
                        StoreCredit = storeCreditDb,
                        StoreCreditHistories = storeCreditHistoryDb.Items
                    };
                }
            }
            catch (Exception ex) 
            { 
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public Task<AppActionResult> RefundReservation(Guid reservationId)
        {
            throw new NotImplementedException();
        }
    }
}
