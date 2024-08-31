using Microsoft.AspNetCore.Mvc;
using ToDo.Application.Common.ResponseModels;
using ToDo.Application.Dtos;
using ToDo.Application.Interface;

namespace ToDo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("user-registration")]
        public async Task<ActionResult<ResultModel>> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            var result = await _authService.RegisterUserAsync(registerUserDto);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("user-login")]
        public async Task<ActionResult<ResultModel<TokenModel>>> LoginUserAsync(LoginUserDto loginUserDto)
        {
            var result = await _authService.LoginUserAsync(loginUserDto);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ResultModel<TokenModel>>> RefreshTokenAsync(TokenModel tokenModel)
        {
            var result = await _authService.ValidateAndGenerateRefreshToken(tokenModel);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
