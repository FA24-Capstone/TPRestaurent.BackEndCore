using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TripInfo
    {
        public class Root
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("trips")]
            public List<Trip> Trips { get; set; }

            [JsonProperty("waypoints")]
            public List<Waypoint> Waypoints { get; set; }
        }

        public class Trip
        {
            [JsonProperty("distance")]
            public double Distance { get; set; }

            [JsonProperty("duration")]
            public double Duration { get; set; }

            [JsonProperty("geometry")]
            public string Geometry { get; set; }

            [JsonProperty("legs")]
            public List<Leg> Legs { get; set; }

            [JsonProperty("weight")]
            public double Weight { get; set; }

            [JsonProperty("weight_name")]
            public string WeightName { get; set; }
        }

        public class Leg
        {
            [JsonProperty("distance")]
            public double Distance { get; set; }

            [JsonProperty("duration")]
            public double Duration { get; set; }

            [JsonProperty("steps")]
            public List<object> Steps { get; set; }

            [JsonProperty("summary")]
            public string Summary { get; set; }

            [JsonProperty("weight")]
            public double Weight { get; set; }
        }

        public class Waypoint
        {
            [JsonProperty("distance")]
            public double Distance { get; set; }

            [JsonProperty("location")]
            public List<double> Location { get; set; }

            [JsonProperty("place_id")]
            public string PlaceId { get; set; }

            [JsonProperty("trips_index")]
            public int TripsIndex { get; set; }

            [JsonProperty("waypoint_index")]
            public int WaypointIndex { get; set; }
        }
    }
}

