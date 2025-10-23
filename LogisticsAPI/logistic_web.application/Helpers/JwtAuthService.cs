using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace logistic_web.application.Helpers
{
    public class JwtAuthService
    {
        private readonly string? _key;
        private readonly string? _issuer;
        private readonly string? _audience;
        private readonly int _expiryInDays;
        private readonly LogisticContext _context;
        private readonly ILogger<JwtAuthService> _logger;

        public JwtAuthService(IConfiguration configuration, LogisticContext context, ILogger<JwtAuthService> logger)
        {
            _key = configuration["Jwt:SecretKey"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
            int.TryParse(configuration["Jwt:ExpiryInDays"], out _expiryInDays);
            _context = context;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            try
            {
                // Khóa bí mật để ký token
                var key = Encoding.ASCII.GetBytes(_key);
                
                // Tạo danh sách các claims cho token
                var claims = new List<Claim>
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.Username),
                    new Claim("Email", user.Email),
                    new Claim("FullName", user.FullName),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Thêm claims cho roles
                var userRoles = _context.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UserId == user.Id)
                    .ToList();

                if (userRoles.Any())
                {
                    foreach (var userRole in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                    }
                }

                // Tạo khóa bí mật để ký token
                var credentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                );

                // Thiết lập thông tin cho token
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(_expiryInDays), // Token hết hạn theo config
                    SigningCredentials = credentials,
                    Issuer = _issuer,
                    Audience = _audience 
                };

                // Tạo token bằng JwtSecurityTokenHandler
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                
                _logger.LogInformation("Token generated successfully for user: {Username}", user.Username);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for user: {Username}", user.Username);
                throw;
            }
        }

        public string DecodePayloadToken(string token)
        {
            try
            {

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserName");
                if (usernameClaim == null)
                {
                    throw new InvalidOperationException("Không tìm thấy username trong payload");
                }

                _logger.LogInformation("Token decoded successfully for user: {Username}", usernameClaim.Value);
                return usernameClaim.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding token");
                throw new InvalidOperationException($"Lỗi khi decode token: {ex.Message}", ex);
            }
        }

        public int GetUserIdFromToken(string token)
        {
            try
            {

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserId");
                if (userIdClaim == null)
                {
                    throw new InvalidOperationException("Không tìm thấy UserId trong payload");
                }

                if (int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                throw new InvalidOperationException("UserId không hợp lệ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from token");
                throw new InvalidOperationException($"Lỗi khi lấy UserId từ token: {ex.Message}", ex);
            }
        }

        public List<string> GetUserRolesFromToken(string token)
        {
            try
            {

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var roleClaims = jwtToken.Claims
                    .Where(x => x.Type == ClaimTypes.Role)
                    .Select(x => x.Value)
                    .ToList();

                return roleClaims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles from token");
                throw new InvalidOperationException($"Lỗi khi lấy roles từ token: {ex.Message}", ex);
            }
        }
    }
}
