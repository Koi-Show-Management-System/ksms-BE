using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KSMS.Domain.Common;
using KSMS.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace KSMS.Infrastructure.Utils
{
    public static class JwtUtil
    {
        public static string GenerateJwtToken(Account account)
        {
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            SymmetricSecurityKey secrectKey =
                new(Encoding.UTF8.GetBytes(AppConfig.JwtSetting.Key));
            var credentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256Signature);
            List<Claim> claims =
            [
                new Claim("Id", account.Id.ToString()),
                new Claim("Email", account.Email),
                new Claim(ClaimTypes.Role, account.Role.Name),
            ];
            var expires = DateTime.Now.AddDays(30);
            var token = new JwtSecurityToken(AppConfig.JwtSetting.Issuer, AppConfig.JwtSetting.Audience, claims, notBefore: DateTime.Now, expires, credentials);
            return jwtHandler.WriteToken(token);
        }
    }
}
