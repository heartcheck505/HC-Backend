using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HeartCheck.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HeartCheck.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(string userId, string email, string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
