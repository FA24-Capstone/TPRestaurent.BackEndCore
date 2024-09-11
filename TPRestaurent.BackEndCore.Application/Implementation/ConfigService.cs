using Castle.DynamicProxy.Generators;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task ChangeConfigurationJob()
        {
            try 
            {
                var utility = Resolve<Utility>();
                var configDb = await _repository.GetAllDataByExpression(null, 0, 0, null, false, null);
                //if (configDb.Items.Count > 0 && configDb.Items != null)
                //{
                //    foreach (var config in configDb.Items)
                //    {
                //        if (config.ActiveDate >= utility!.GetCurrentDateTimeInTimeZone())
                //        {
                //            if (config.ActiveValue != null)
                //            {
                //                config.PreValue = config!.ActiveValue!;
                //                config.ActiveValue = null;
                //                config.ActiveDate = null;
                //                await _repository.Update(config);   
                //            }
                //        }
                //    }
                //    await _unitOfWork.SaveChangesAsync();       
                //}
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
                };

                await _repository.Insert(configuration);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
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
                    result = BuildAppActionResultError(result, $"Không tìm thấy thông số cấu hình với tên {name}");
                }
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
            //try
            //{
            //    var configurationDb = await _repository.GetById(dto.ConfigurationId);
            //    if (configurationDb == null)
            //    {
            //        return BuildAppActionResultError(result, $"Không tìm thấy cấu hình với id {dto.ConfigurationId}");
            //    }
            //    if (dto.ActiveDate.HasValue)
            //    {
            //        configurationDb.ActiveDate = dto.ActiveDate.Value;
            //    }
            //    else
            //    {
            //        if (string.IsNullOrEmpty(dto.ActiveValue))
            //        {
            //            var utility = Resolve<Utility>();
            //            configurationDb.ActiveDate = utility.GetCurrentDateTimeInTimeZone();
            //        }
            //    }

            //    configurationDb.PreValue = dto.PreValue;
            //    configurationDb.ActiveValue = dto.ActiveValue;
            //    await _repository.Update(configurationDb);
            //    await _unitOfWork.SaveChangesAsync();

            //}
            //catch (Exception ex)
            //{
            //    result = BuildAppActionResultError(result, ex.Message);
            //}
            return result;
        }

    }
}
