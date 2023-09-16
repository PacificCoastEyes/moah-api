using moah_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace moah_api.Utilities
{
    public class TokenSigner
    {
        private readonly IConfiguration _configuration;
        public TokenSigner(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string SignToken(User user)
        {

            var claims = new List<Claim>
            {
                new("id", user.Id.ToString()),
                new("firstName", user.FirstName!.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("TokenSecret")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}