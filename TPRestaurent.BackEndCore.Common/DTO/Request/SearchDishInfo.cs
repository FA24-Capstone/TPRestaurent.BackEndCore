namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class SearchDishInfo
    {
        public List<string> Tags { get; set; }
        public string Name { get; set; }
        public (decimal? Min, decimal? Max) PriceRange { get; set; }
    }
}