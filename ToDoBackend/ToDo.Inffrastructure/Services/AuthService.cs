using Microsoft.AspNetCore.Identity;
using ToDo.Application.Common;
using ToDo.Application.Dtos;
using ToDo.Application.Interface;
using ToDo.Domain.Models;

namespace ToDo.Inffrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<ResultModel> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
           
            if (registerUserDto.Password != registerUserDto.ConfirmPassword)
            {
                return ResultModel.ErrorResult("Passwords do not match.");
            }
            var existingUserByEmail = await _userManager.FindByEmailAsync(registerUserDto.Email); 
            if (existingUserByEmail is not null)
            {
                return ResultModel.ErrorResult($"User is already registerd with email {registerUserDto.Email}");
            }

            var existingUserByUsername = await _userManager.FindByNameAsync(registerUserDto.Username);
            if (existingUserByUsername != null)
            {
                return ResultModel.ErrorResult($"Username '{registerUserDto.Username}' is already taken.");
            }

            var newUser = new AppUser()
            {
                Email = registerUserDto.Email,
                UserName = registerUserDto.Username
            };

            var result = await _userManager.CreateAsync(newUser,registerUserDto.Password);

            if (result.Succeeded)
            {
                return ResultModel.SuccessResult($"UserUser {newUser.UserName} is registered successfully with email {newUser.Email}");
            }

            return ResultModel.ErrorResult($"User registration failed.");

        }

        public async Task<ResultModel> LoginUserAsync(LoginUserDto loginUserDto)
        { 

            
            var existingUser = await _userManager.FindByEmailAsync(loginUserDto.Username);
            if(existingUser is null)
            {
                existingUser = await _userManager.FindByNameAsync(loginUserDto.Username);
            }
            if(existingUser is null)
            {
              return ResultModel.ErrorResult($"user {loginUserDto.Username} does not exist"); 
            }
            var result = await _signInManager.CheckPasswordSignInAsync(existingUser, loginUserDto.Password, false);
            if (result.Succeeded)
            {
                return ResultModel.SuccessResult($"User {loginUserDto.Username} logged in successfully");
            }
            else
            {
                return ResultModel.SuccessResult($"Failed to login user {loginUserDto.Username} ");
            }
            
        }
    }
}
