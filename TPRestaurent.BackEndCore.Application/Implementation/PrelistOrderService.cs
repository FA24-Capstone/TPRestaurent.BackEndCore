using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class PrelistOrderService : GenericBackendService, IPreListOrderService
    {
        private IGenericRepository<PrelistOrder> _repository;
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;
        public PrelistOrderService(IGenericRepository<PrelistOrder> repository, IMapper mapper, IUnitOfWork unitOfWork, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> AddReservationDishToPrelistOrder(DateTime? time)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                AppActionResult result = new AppActionResult();
                try
                {
                    if (time.HasValue)
                    {
                        var utility = Resolve<Utility>();
                        time = utility.GetCurrentDateInTimeZone();
                    }
                    var reservationRepository = Resolve<IGenericRepository<Reservation>>();
                    //Replace with configuration
                    var reservationDb = await reservationRepository!.GetAllDataByExpression(r => r.ReservationDate.AddHours(0.5) >= time && r.StatusId == Domain.Enums.ReservationStatus.PAID, 0, 0, null, false, null);
                    if (reservationDb.Items!.Count > 0)
                    {
                        var reservationIds = reservationDb.Items.Select(i => i.ReservationId).ToList();
                        var prelistExisted = await _repository.GetAllDataByExpression(p => p.ReservationDishId.HasValue && reservationIds.Contains(p.ReservationDish!.ReservationId!.Value), 0, 0, null, false, null);

                        if (prelistExisted.Items!.Count == 0)
                        {
                            var reservationDishRepository = Resolve<IGenericRepository<ReservationDish>>();
                            var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                            List<PrelistOrder> prelistOrders = new List<PrelistOrder>();
                            List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();

                            var reservationDishDb = await reservationDishRepository!.GetAllDataByExpression(r => reservationIds.Contains(r.ReservationId.Value), 0, 0, null, false, null);
                            Guid prelistOrderId = Guid.Empty;

                            foreach (var item in reservationDishDb.Items!)
                            {
                                prelistOrderId = Guid.NewGuid();
                                prelistOrders.Add(new PrelistOrder
                                {
                                    PrelistOrderId = prelistOrderId,
                                    ReservationDishId = item.ReservationDishId,
                                    OrderTime = time!.Value,
                                    Quantity = item.Quantity,
                                });

                                var comboOrderDetailDb = await comboOrderDetailRepository!.GetAllDataByExpression(c => c.ReservationDishId == item.ReservationDishId, 0, 0, null, false, null);
                                comboOrderDetailDb.Items!.ForEach(c => c.PrelistOrderId = prelistOrderId);
                                comboOrderDetails.AddRange(comboOrderDetailDb.Items);
                            }
                            await _repository.InsertRange(prelistOrders);
                            await comboOrderDetailRepository!.UpdateRange(comboOrderDetails);
                        }
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
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
}
