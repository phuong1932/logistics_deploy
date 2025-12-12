using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace logistic_web.application.Helpers
{
    public class JwtAuthService
    {
        private readonly string? _key;
        private readonly string? _issuer;
        private readonly string? _audience;
        private readonly int _expiryInDays;
        private readonly ILogger<JwtAuthService> _logger;

        public JwtAuthService(IConfiguration configuration, ILogger<JwtAuthService> logger)
        {
            _key = configuration["Jwt:SecretKey"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
            int.TryParse(configuration["Jwt:ExpiryInDays"], out _expiryInDays);

            _logger = logger;
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
        public string GetRoleFromToken(string token)
        {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
    
                var roleClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "role");  
                if (roleClaim == null)
                {
                    throw new InvalidOperationException("Không tìm thấy Role trong payload");
                }

                return roleClaim.Value;
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
        public  string GetHomePageByRole(string token)
        {
            try
            {
                var roles = GetRoleFromToken(token);

                if (roles.Contains("admin") || roles.Contains("staff"))
                {
                    return "/dashboard";
                }
                else if (roles.Contains("shipper"))
                {
                    return "/shipper-cargo-list";
                }

                return "/dashboard"; // default
            }
            catch
            {
                return "/login";
            }
        }
    }
}
