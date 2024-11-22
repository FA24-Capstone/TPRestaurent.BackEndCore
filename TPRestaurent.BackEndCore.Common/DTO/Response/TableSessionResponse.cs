using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TableSessionResponse
    {
        //public TableSession TableSession { get; set; } = null!;
        public List<PrelistOrderDetails> UncheckedPrelistOrderDetails { get; set; } = new List<PrelistOrderDetails>();

        public List<PrelistOrderDetails> ReadPrelistOrderDetails { get; set; } = new List<PrelistOrderDetails>();
        public List<PrelistOrderDetails> ReadyToServePrelistOrderDetails { get; set; } = new List<PrelistOrderDetails>();
    }

    public class PrelistOrderDetails
    {
        //public PrelistOrder PrelistOrder { get; set; } = null!;
        public List<ComboOrderDetail>? ComboOrderDetails { get; set; } = new List<ComboOrderDetail>();
    }
}