﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("table")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private ITableService _service;
        public TableController(ITableService service)
        {
            _service = service;
        }

        [HttpGet("get-all-table/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllTable(int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllTable(pageNumber, pageSize);
        }

        [HttpPost("create-table")]
        public async Task<AppActionResult> CreateTable([FromBody]TableDto dto)
        {
            return await _service.CreateTable(dto);
        }
    }
}