using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
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

        public async Task<AppActionResult> GetStoreCreditByAccountId(string accountId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var storeCreditDb = await _repository.GetByExpression(s => s.CustomerInfo.AccountId == accountId, null);
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
    }
}
