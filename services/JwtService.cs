using BankLite.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankLite.Api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _cfg;
        public JwtService(IConfiguration cfg) => _cfg = cfg;

        public string CreateToken(User user)
        {
            var key = _cfg["Jwt:Key"] ?? throw new Exception("Jwt:Key missing");
            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var expMin = int.TryParse(_cfg["Jwt:ExpiresMinutes"], out var m) ? m : 120;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("fullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expMin),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
