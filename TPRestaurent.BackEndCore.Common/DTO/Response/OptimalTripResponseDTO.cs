using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OptimalTripResponseDTO
    {
        public List<RouteNode> OptimalTrip { get; set; } = new List<RouteNode>();
        public double TotalDistance { get; set; }
        public double TotalDuration { get; set; }
    }

    public class RouteNode
    {
        public int Index { get; set; }
        public string AccountId { get; set; } = null!;
        public List<Order> Orders { get; set; } = null!;
        public string Duration { get; set; }
        public string DistanceFromPreviousDestination { get; set; }
    }
}