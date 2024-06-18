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

namespace QuizWhiz.Domain.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;
      
        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(string email, string password, string role, string userName)
        {
            var claims = new[]
           {
                new Claim("Email", email),
                new Claim("Role", role),
                new Claim("Password", password),
                new Claim ("Username",userName )
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
    }
}
