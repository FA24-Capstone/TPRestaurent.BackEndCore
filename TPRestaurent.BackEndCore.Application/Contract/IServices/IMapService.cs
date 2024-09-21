using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IMapService
    {
        public Task<AppActionResult> Geocode(string address);
        public Task<AppActionResult> AutoComplete(string address);
        public Task<AppActionResult> GetEstimateDeliveryTime(double[] endCoordinate, double[]? startCoordinate);

    }
}
