using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Infrastructure.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RentIt.Users.Infrastructure.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IDistributedCache _cache;

        public JwtProvider(IOptions<JwtOptions> options, IDistributedCache cache)
        {
            _jwtOptions = options.Value;
            _cache = cache;
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            var jti = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName.ToString())
            };

            var signingCreds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.TokenLifetimeMinutes),
                signingCredentials: signingCreds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var key = $"access:{jti}";
            await _cache.SetStringAsync(key, tokenString, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_jwtOptions.TokenLifetimeMinutes)
            });

            return tokenString;
        }

        public async Task<string> GenerateRefreshTokenAsync(User? user)
        {
            var jti = Guid.NewGuid().ToString();
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            var refreshToken = Convert.ToBase64String(randomNumber);

            var key = $"refresh:{refreshToken}";
            await _cache.SetStringAsync(key, refreshToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_jwtOptions.RefreshTokenLifetimeDays)
            });

            return refreshToken;
        }

        public async Task RevokeAccessTokenAsync(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.ReadToken(accessToken) is JwtSecurityToken jwtToken)
            {
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrEmpty(jti))
                {
                    await _cache.RemoveAsync($"access:{jti}");
                }
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            await _cache.RemoveAsync($"refresh:{refreshToken}");
        }

        public async Task<string> GetStoredTokenAsync(string jti)
        {
            return await _cache.GetStringAsync($"refresh:{jti}") 
                ?? await _cache.GetStringAsync($"access:{jti}");
        }
    }
}
