using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TableSessionService : GenericBackendService, ITableSessionService
    {
        private IGenericRepository<TableSession> _tableSessionRepository;
        private IGenericRepository<PrelistOrder> _prelistOrderRepository;
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;
        public TableSessionService(IGenericRepository<TableSession> tableSessionRepository, 
                                   IGenericRepository<PrelistOrder> prelistOrderRepository,
                                   IMapper mapper,
                                   IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _tableSessionRepository = tableSessionRepository;
            _prelistOrderRepository = prelistOrderRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public Task<AppActionResult> AddNewPrelistOrder(List<PrelistOrderDto> dto)
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> AddTableSession(TableSessionDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> GetLatestPrelistOrder(double? minute, bool IsOrdered)
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> GetTableSessionById(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> UpdatePrelistOrderStatus(List<Guid> prelistOrderIds)
        {
            throw new NotImplementedException();
        }
    }
}
