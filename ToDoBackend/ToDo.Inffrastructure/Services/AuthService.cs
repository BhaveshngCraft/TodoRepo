using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ToDo.Application.Common.ResponseModels;
using ToDo.Application.Constants;
using ToDo.Application.Dtos;
using ToDo.Application.Interface;
using ToDo.Application.Options;
using ToDo.Domain.Models;
using ToDo.Persistenece.Data;

namespace ToDo.Inffrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly DataContext _dataContext;
        private readonly JwtOptions _jwtOptions;
        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, DataContext dataContext, IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;         
            _dataContext = dataContext;
            _jwtOptions = jwtOptions.Value;
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
                UserName = registerUserDto.Username,
                EmailConfirmed = true
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

                await SetRefreshToken(refreshToken,existingUser.Id);
               
                return ResultModel<TokenModel>.SuccessResult($"User {loginUserDto.Username} logged in successfully", tokenModel);
            }
           
                return ResultModel<TokenModel>.ErrorResult($"Failed to login user {loginUserDto.Username} ", new TokenModel());
            
        }

        private async Task<string> GenerateAccessToken(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
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

            var token = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(_jwtOptions.TokenValidityInMinutes),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<ResultModel<TokenModel>> ValidateAndGenerateRefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return ResultModel<TokenModel>.ErrorResult($"Invalid token request", new TokenModel());
            }

            string accessToken = tokenModel.AccessToken!;
            string refreshToken = tokenModel.RefreshToken!;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return ResultModel<TokenModel>.ErrorResult($"Invalid access token request", new TokenModel());
            }

            string username = principal.Identity!.Name!;
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return ResultModel<TokenModel>.ErrorResult($"User {user!.UserName} does not exist OR invalid user", new TokenModel());
            }
            var userRefreshToken = user.RefreshTokens.Where(x => x.IsRevoked == false && x.ExpiresAt <= DateTime.UtcNow && x.Token == refreshToken);
            if(userRefreshToken is null)
            {
                return ResultModel<TokenModel>.ErrorResult($"Invalid refresh token OR refresh token does not exists", new TokenModel());
            }

            tokenModel = new TokenModel
            {
                AccessToken = await GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(),
            };

            await SetRefreshToken(tokenModel.RefreshToken, user.Id);

            return ResultModel<TokenModel>.SuccessResult($"New access token and refresh token generated successfully", tokenModel);
            
            
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;

        }

        private async Task<bool> SetRefreshToken(string refreshToken , string userId)
        {
            
            var refreshTokenList = await _dataContext.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();

            refreshTokenList.ForEach(x => x.IsRevoked = true);

            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(_jwtOptions.RefreshTokenValidityInDays)),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false

            };

            _dataContext.RefreshTokens.Add(newRefreshToken);
            await _dataContext.SaveChangesAsync();
            return true;
        }
    }
}
