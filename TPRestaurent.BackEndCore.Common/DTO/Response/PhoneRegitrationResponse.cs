using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class PhoneRegitrationResponse
    {
        public Account Account { get; set; }
        public string Code { get; set; }
    }
}