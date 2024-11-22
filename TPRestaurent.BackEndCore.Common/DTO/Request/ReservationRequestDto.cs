namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ReservationRequestDto
    {
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime? EndTime { get; set; }
        public Guid CustomerAccountId { get; set; }
        public string? Note { get; set; }
    }
}