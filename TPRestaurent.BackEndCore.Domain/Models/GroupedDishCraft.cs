namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class GroupedDishCraft
    {
        public Guid GroupedDishCraftId { get; set; }
        public int GroupNumber { get; set; }
        public bool IsFinished { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string OrderDetailidList { get; set; }
        public string GroupedDishJson { get; set; }
    }
}