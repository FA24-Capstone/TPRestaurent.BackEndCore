using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Order Order { get; set; } = null!;
        public string Duration { get; set; }
        public string DistanceToNextDestination { get; set; }
    }
}
