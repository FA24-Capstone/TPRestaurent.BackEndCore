namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class MapAutoCompleteRequestDto
    {
        public string Address { get; set; }
        public double[]? Destination { get; set; } = new double[2];
    }
}