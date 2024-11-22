namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class GetEstimateTimeRequest
    {
        public double[] desc { get; set; } = new double[2];
        public double[]? start { get; set; }
    }
}