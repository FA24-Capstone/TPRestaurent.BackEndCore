namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class AssignCouponRequestDto
    {
        public Guid CouponProgramId { get; set; }
        public List<string> CustomerIds { get; set; } = new List<string>();
    }
}