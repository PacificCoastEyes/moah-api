using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace moah_api.Utilities
{
    public class TokenDecryptor
    {
        private readonly IConfiguration _configuration;
        public TokenDecryptor(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ClaimsPrincipal DecryptToken(string token)
        {
            var TokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["TokenSecret"]!)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            ClaimsPrincipal payload = TokenHandler.ValidateToken(token, validationParameters, out _);
            return payload;
        }
    }
}