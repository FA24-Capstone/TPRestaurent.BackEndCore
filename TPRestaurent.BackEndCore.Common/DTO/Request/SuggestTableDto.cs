namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class SuggestTableDto
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int NumOfPeople { get; set; }
        public bool IsPrivate { get; set; }
    }
}