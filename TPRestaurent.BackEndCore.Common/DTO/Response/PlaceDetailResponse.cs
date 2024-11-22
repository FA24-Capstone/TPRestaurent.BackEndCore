namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class PlaceDetailResponse
    {
        public Result result { get; set; }
        public string status { get; set; }
    }

    public class Geometry
    {
        public Location location { get; set; }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Result
    {
        public string place_id { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public string name { get; set; }
    }
}