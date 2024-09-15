using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TokenService : GenericBackendService ,ITokenService
    {
        private readonly IGenericRepository<Token> _tokenRepository;  
        public TokenService(IServiceProvider serviceProvider, IGenericRepository<Token> tokenRepository) : base(serviceProvider)
        {
            _tokenRepository = tokenRepository; 
        }

        public async Task<AppActionResult> GetUserToken(string token)
        {
            var result = new AppActionResult();
            var utility = Resolve<Utility>();
            try
            {
                var tokenDb = await _tokenRepository.GetByExpression(p => p.AccessTokenValue == token, p => p.Account!);
                if (tokenDb == null)
                {
                    result = BuildAppActionResultError(result, $"Token này không tồn tại");
                }
                result.Result = tokenDb;    
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        //public async Task<AppActionResult> InvalidateTokensForUser(string accountId)
        //{
        //    var result = new AppActionResult();
        //    var utility = Resolve<Utility>();
        //    try
        //    {
        //        var accountTokenList = await _tokenRepository.GetAllDataByExpression(p => p.AccountId == accountId, 0, 0, null, false, p => p.Account!);
        //        if (accountTokenList == null)
        //        {
        //            result = BuildAppActionResultError(result, $"Danh sách token của tài khoản {accountId} không tồn tại");
        //        }
        //        if (accountTokenList!.Items!.Count < 0 && accountTokenList.Items != null)
        //        {
        //            foreach (var token in accountTokenList.Items)
        //            {
        //                token.IsActive = false;     
        //                token.ExpiryTimeAccessToken = utility!.GetCurrentDateTimeInTimeZone();
        //                token.ExpiryTimeRefreshToken = utility.GetCurrentDateTimeInTimeZone();      
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}
    }
}
