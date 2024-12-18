using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IInvoiceService
    {
        public Task GenerateInvoice();

        public Task<AppActionResult> GetAllInvoice(InvoiceFilterRequest dto);

        public Task<AppActionResult> GenerateGeneralInvoice(InvoiceFilterRequest dto);

        public Task<AppActionResult> GetInvoiceById(Guid id);
        public Task<AppActionResult> GetInvoicebyOrderId(Guid orderId);
    }
}