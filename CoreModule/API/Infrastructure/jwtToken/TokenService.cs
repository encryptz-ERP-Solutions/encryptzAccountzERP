using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Jwt
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string, DateTime) GenerateAccessToken(string userId, string userHandle, Dictionary<Guid, IEnumerable<string>> permissionsByBusiness)
        {
            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]);
            var expirationMinutes = Convert.ToInt32(_config["JwtSettings:AccessTokenExpirationMinutes"]);
            var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, userHandle)
            };

            // Add permissions as claims.
            // A more complex scenario might format this as "businessId:permissionKey"
            // For now, we add each permission key as a separate "permission" claim.
            if (permissionsByBusiness != null)
            {
                foreach (var businessPermissions in permissionsByBusiness)
                {
                    foreach (var permission in businessPermissions.Value)
                    {
                        claims.Add(new Claim("permission", permission));
                    }
                }
            }

            // If a user has any permissions, we can assign them a generic "User" role.
            // A more advanced system could have distinct roles like "GlobalAdmin".
            if (claims.Any(c => c.Type == "permission"))
            {
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return (tokenString, expires);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}