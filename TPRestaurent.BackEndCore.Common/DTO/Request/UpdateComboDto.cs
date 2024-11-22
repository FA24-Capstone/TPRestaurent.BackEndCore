namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateComboDto
    {
        public Guid ComboId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DishComboDto> DishComboDtos { get; set; } = new List<DishComboDto>();
        public List<Guid> TagIds { get; set; } = new List<Guid>();
        public int? PreparationTime { get; set; }
    }
}