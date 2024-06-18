using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using QuizWhiz.Domain.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizWhiz.Domain.Helpers
{
    public class CustomAuthorize : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _role;
      
        public CustomAuthorize(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Split(" ").Last();
            if (!IsTokenValid(token, out var claimsPrincipal))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Set the user principal for the current context
            context.HttpContext.User = claimsPrincipal;

            var userRole = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == null || userRole != _role)
            {
                context.Result = new ForbidResult();
            }
        }

        public bool IsTokenValid(string token, out ClaimsPrincipal claimsPrincipal)
        {
            claimsPrincipal = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("This Is My Secret Key For Authorization");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // More validation parameters if needed
                };

                claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return validatedToken != null;
            }
            catch
            {
                return false;
            }
        }

    }
}
