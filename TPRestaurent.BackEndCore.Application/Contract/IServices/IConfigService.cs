﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IConfigService
    {
        public Task<AppActionResult> GetByName(string name);
        public Task<AppActionResult> GetAll(int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllConfigurationVersion(int pageNumber, int pageSize);
        public Task<AppActionResult> CreateConfigurationVersion(ConfigurationVersionDto configurationVersionDto);
        public Task<AppActionResult> CreateConfiguration(ConfigurationDto dto);
        public Task<AppActionResult> UpdateConfiguration(UpdateConfigurationDto dto);
        public Task ChangeConfigurationJob();
    }
}
