namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ReservationDto
    {
        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime? EndTime { get; set; }
        public Guid? CustomerId { get; set; }
        public double Deposit { get; set; }
        public string? Note { get; set; }
        public bool IsPrivate { get; set; }
        public List<ReservationDishDto>? ReservationDishDtos { get; set; } = new List<ReservationDishDto>();
        //public List<Guid> ReservationTableIds { get; set; } = new List<Guid>();
    }
}