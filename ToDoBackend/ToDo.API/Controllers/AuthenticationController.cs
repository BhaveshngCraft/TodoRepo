using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDo.Application.Common;
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
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

    }
}
