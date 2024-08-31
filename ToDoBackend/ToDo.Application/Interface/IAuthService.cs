using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.Common.ResponseModels;
using ToDo.Application.Dtos;

namespace ToDo.Application.Interface
{
    public interface IAuthService
    {
        Task<ResultModel> RegisterUserAsync(RegisterUserDto registerUserDto);
        Task<ResultModel<TokenModel>> LoginUserAsync(LoginUserDto loginUserDto);
        Task<ResultModel<TokenModel>> ValidateAndGenerateRefreshToken(TokenModel tokenModel);
    }
}
