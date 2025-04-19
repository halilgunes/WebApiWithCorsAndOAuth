using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PublicPersonelDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        /// <summary>
        /// Postman'de http://localhost:5216/api/Auth/token şeklinde deneyip body kısmına raw seçerek { "username": "admin",  "password": "password" } şeklinde gönderip deneyince
        // aşağıdakine benzer çıktı alıyoruz.
        /// {
        /// "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6ImFhNDQ2ZGFlLWQ2MDUtNGNhMC04ZmFjLWY5ZDI4YzJjYTJmMiIsImV4cCI6MTc0MzI5MjA1NiwiaXNzIjoiUHVibGljUGVyc29uZWxEYXRhQXBpIiwiYXVkIjoiUHVibGljUGVyc29uZWxEYXRhQXBpVXNlcnMifQ.cBh3T-vDBoHLGQ4Hh_Y1W-3LbH_MqNyVAoQUS--SDa0"
        ///}
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost("token")]
        public IActionResult GenerateToken([FromBody] LoginModel login)
        {
            // Basit bir kullanıcı doğrulama (gerçek bir kullanıcı doğrulama mekanizması ekleyin)
            if (login.Username != "admin" || login.Password != "password")
            {
                return Unauthorized("Geçersiz kullanıcı adı veya şifre.");
            }

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, login.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }

   
}