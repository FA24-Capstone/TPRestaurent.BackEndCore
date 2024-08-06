﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("store-credit")]
    [ApiController]
    public class StoreCreditController : ControllerBase
    {
        private readonly IStoreCreditService _service;
        public StoreCreditController(IStoreCreditService service)
        {
            _service = service;
        }

        [HttpGet("get-store-credit-by-account-id/{accountId}")]
        public async Task<AppActionResult> Get(string accountId)
        {
            return await _service.GetStoreCreditByAccountId(accountId);
        }
    }
}
