﻿using Microsoft.IdentityModel.Tokens;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ConfigService : GenericBackendService, IConfigService
    {
        private BackEndLogger _logger;
        private IGenericRepository<Configuration> _repository;
        private IUnitOfWork _unitOfWork;
        public ConfigService(IGenericRepository<Configuration> repository, IUnitOfWork unitOfWork, BackEndLogger logger, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        //[Hangfire.Queue("change-configuration")]
        public async Task ChangeConfigurationJob()
        {
            try
            {
                var configurationServiceRepository = Resolve<IGenericRepository<ConfigurationVersion>>();
                var utility = Resolve<Common.Utils.Utility>();
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var configDb = await _repository.GetAllDataByExpression(null, 0, 0, null, false, null);
                if (configDb.Items!.Count > 0 && configDb.Items != null)
                {
                    foreach (var config in configDb.Items)
                    {
                        var configVersionDb = await configurationServiceRepository!.GetAllDataByExpression(p => p.ConfigurationId == config.ConfigurationId && p.ActiveDate <= currentTime, 0, 0, p => p.ActiveDate, false, null);
                        if (configVersionDb.Items.Count > 0 && configVersionDb.Items != null)
                        {
                            var closestConfig = configVersionDb!.Items!
                                   .OrderByDescending(p => p.ActiveDate)
                                   .FirstOrDefault();

                            if (closestConfig != null && !closestConfig.IsApplied)
                            {
                                closestConfig.IsApplied = true;
                                config.CurrentValue = closestConfig!.ActiveValue;
                                await configurationServiceRepository.Update(closestConfig);
                                await _repository.Update(config);
                            }
                        }
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }

        public async Task<AppActionResult> CreateConfiguration(ConfigurationDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var configurationVersionRepository = Resolve<IGenericRepository<ConfigurationVersion>>();
                var utility = Resolve<Common.Utils.Utility>();
                var configurationDb = await _repository.GetByExpression(c => c.Name.Equals(dto.Name), null);
                if (configurationDb != null)
                {
                    return BuildAppActionResultError(result, $"Đã tồn tại cấu hình với tên {dto.Name}");
                }
                var configuration = new Configuration
                {
                    ConfigurationId = Guid.NewGuid(),
                    Name = dto.Name,
                    CurrentValue = dto.CurrentValue,
                    VietnameseName = dto.VietnameseName,
                    Unit = dto.Unit
                };
                var configurationVersion = new ConfigurationVersion
                {
                    ConfigurationVersionId = Guid.NewGuid(),
                    ActiveDate = utility.GetCurrentDateTimeInTimeZone(),
                    ActiveValue = dto.CurrentValue,
                    ConfigurationId = configuration.ConfigurationId,
                    IsApplied = true
                };


                await configurationVersionRepository.Insert(configurationVersion);
                await _repository.Insert(configuration);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public async Task<AppActionResult> CreateConfigurationVersion(ConfigurationVersionDto configurationVersionDto)
        {
            var configurationServiceRepository = Resolve<IGenericRepository<ConfigurationVersion>>();
            AppActionResult result = new AppActionResult();
            try
            {
                var configurationVersion = new ConfigurationVersion
                {
                    ConfigurationVersionId = Guid.NewGuid(),
                    ActiveDate = configurationVersionDto.ActiveDate,
                    ActiveValue = configurationVersionDto.ActiveValue,
                    ConfigurationId = configurationVersionDto.ConfigurationId,
                    IsApplied = false
                };

                await configurationServiceRepository!.Insert(configurationVersion);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAll(int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                result.Result = await _repository.GetAllDataByExpression(null, pageNumber, pageSize, null, false, null);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllConfigurationVersion(int pageNumber, int pageSize)
        {
            var configurationServiceRepository = Resolve<IGenericRepository<ConfigurationVersion>>();
            AppActionResult result = new AppActionResult();
            try
            {
                result.Result = await configurationServiceRepository!.GetAllDataByExpression(null, pageNumber, pageSize, p => p.ActiveDate, false, p => p.Configuration!);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetByName(string name)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var configDb = await _repository.GetByExpression(c => c.Name.Equals(name), null);
                if (configDb != null)
                {
                    result.Result = configDb;
                }
                else
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy thông số cấu hình với tên {name}");
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllConfigurationVersion(Guid configId, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var configurationVersionRepository = Resolve<IGenericRepository<ConfigurationVersion>>();
            try
            {
                result.Result = await configurationVersionRepository!.GetAllDataByExpression(p => p.ConfigurationId == configId, pageNumber, pageSize, p => p.ActiveDate, false, p => p.Configuration!);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateConfiguration(UpdateConfigurationDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Common.Utils.Utility>();
                var configurationVersionRepository = Resolve<IGenericRepository<ConfigurationVersion>>();
                var configurationDb = await _repository.GetById(dto.ConfigurationId);
                if (configurationDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy cấu hình với id {dto.ConfigurationId}");
                }

                if (!string.IsNullOrEmpty(dto.CurrentValue))
                {
                    configurationDb.CurrentValue = dto.CurrentValue;
                }

                if (!string.IsNullOrEmpty(dto.Name))
                {
                    configurationDb.Name = dto.Name;
                }

                if (!string.IsNullOrEmpty(dto.VietnameseName))
                {
                    configurationDb.VietnameseName = dto.VietnameseName;
                }

                if (!string.IsNullOrEmpty(dto.Unit))
                {
                    configurationDb.Unit = dto.Unit;
                }

                var configurationVersion = new ConfigurationVersion
                {
                    ConfigurationVersionId = Guid.NewGuid(),
                    ActiveDate = utility.GetCurrentDateTimeInTimeZone(),
                    ActiveValue = dto.CurrentValue,
                    ConfigurationId = dto.ConfigurationId,
                    IsApplied = true
                };

                
                await configurationVersionRepository.Insert(configurationVersion);
                await _repository.Update(configurationDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}