using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IStoreCreditService
    {
        //public Task<AppActionResult> GetStoreCreditByAccountId(string accountId);
        public Task<AppActionResult> AddStoreCredit(Guid transactionId);

        public Task<AppActionResult> RefundReservation(Guid reservationId);

        public Task ChangeOverdueStoreCredit();
    }
}