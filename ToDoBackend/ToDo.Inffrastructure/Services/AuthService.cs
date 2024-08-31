using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ToDo.Application.Common.ResponseModels;
using ToDo.Application.Constants;
using ToDo.Application.Dtos;
using ToDo.Application.Interface;
using ToDo.Domain.Models;
using ToDo.Persistenece.Data;

namespace ToDo.Inffrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly DataContext _dataContext;
        private readonly IConfiguration _config;
        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration config, DataContext dataContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _dataContext = dataContext;
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

            var result = await _userManager.CreateAsync(newUser, registerUserDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);
                return ResultModel.SuccessResult($"UserUser {newUser.UserName} is registered successfully with email {newUser.Email}");
            }

            return ResultModel.ErrorResult($"User registration failed.");

        }

        public async Task<ResultModel<TokenModel>> LoginUserAsync(LoginUserDto loginUserDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(loginUserDto.Username) ??
                await _userManager.FindByNameAsync(loginUserDto.Username);
         
            if (existingUser is null)
            {
                return ResultModel<TokenModel>.ErrorResult($"user {loginUserDto.Username} does not exist", new TokenModel());
            }
            var result = await _signInManager.CheckPasswordSignInAsync(existingUser, loginUserDto.Password, false);
            if (result.Succeeded)
            {
                var accessToken = await GenerateAccessToken(existingUser);
                var refreshToken = GenerateRefreshToken();

                var tokenModel = new TokenModel
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                var addRefreshToken = new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    UserId = existingUser.Id
                };
                 _dataContext.RefreshTokens.Add(addRefreshToken);
                await _dataContext.SaveChangesAsync();
               
                return ResultModel<TokenModel>.SuccessResult($"User {loginUserDto.Username} logged in successfully", new TokenModel());
            }
            else
            {
                return ResultModel<TokenModel>.ErrorResult($"Failed to login user {loginUserDto.Username} ", new TokenModel());
            }
        }

        private async Task<string> GenerateAccessToken(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
