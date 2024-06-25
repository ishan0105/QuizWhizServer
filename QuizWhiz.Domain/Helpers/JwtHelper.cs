using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuizWhiz.Application.DTOs.Response;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace QuizWhiz.Domain.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateJwtToken(string email, string role, string username)
        {
            var claims = new[]
           {
                new Claim("Email", email),
                new Claim("Role", role),
                new Claim ("Username",username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenDTO DecodeToken()
        {
            var context = _httpContextAccessor.HttpContext;
            var token = context.Request.Headers["Authorization"].ToString().Split(" ").Last();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("nTB981AWJOmY44dpCDcCuwYO6nuXcFAk98B$7SutWNVEe+truifreDSGHJooierAEWdfgDSFd");
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // More validation parameters if needed
            };

            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var userRole = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;

            if (userRole != null)
            {
                TokenDTO tokenDTO = new()
                {
                    Username = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Username")?.Value,
                    UserRole = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Role")?.Value,
                    Email = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Email")?.Value,
                };

                return tokenDTO;
            }

            return null;
        }
    }
}
