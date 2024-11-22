namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class KitchenTableSimpleResponse
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public Guid OrderId { get; set; }
        public int UnCheckedNumberOfDishes { get; set; }
    }
}