using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Application.Common;
using ToDo.Application.Dtos;

namespace ToDo.Application.Interface
{
    public interface IAuthService
    {
        Task<ResultModel> RegisterUserAsync(RegisterUserDto registerUserDto);
    }
}
