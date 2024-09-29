﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IMapService
    {
        public Task<double[]> GetPlaceById(string id);
        public Task<AppActionResult> Geocode(string address);
        public Task<AppActionResult> AutoComplete(MapAutoCompleteRequestDto dto);
        public Task<AppActionResult> GetEstimateDeliveryResponse(double[] endCoordinate, double[]? startCoordinate);
    }
}
