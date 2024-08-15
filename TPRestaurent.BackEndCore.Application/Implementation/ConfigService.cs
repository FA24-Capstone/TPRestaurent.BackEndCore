using Castle.DynamicProxy.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ConfigService : GenericBackendService, IConfigService
    {
        private IGenericRepository<Configuration> _repository;
        private IUnitOfWork _unitOfWork;
        public ConfigService(IGenericRepository<Configuration> repository, IUnitOfWork unitOfWork, IServiceProvider service) : base(service) 
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> CreateConfiguration(ConfigurationDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var configuration = new List<Configuration>();
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

        public Task<AppActionResult> UpdateConfiguration(ConfigurationDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
