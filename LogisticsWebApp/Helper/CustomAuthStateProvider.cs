using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace LogisticsWebApp.Helper
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        // Thêm biến global ở đây
        public static string UserNameLoginCurrent { get; set; } = "User";

        public CustomAuthStateProvider(ILocalStorageService localStorage, IConfiguration configuration)
        {
            _localStorage = localStorage;
            _configuration = configuration;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("token");
                
                if (string.IsNullOrWhiteSpace(token))
                {
                    UserNameLoginCurrent = "User"; // Cập nhật khi không có token
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var secretKey = _configuration["Jwt:SecretKey"];
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                
                // Cập nhật username từ token
                var usernameClaim = principal.Claims.FirstOrDefault(x => x.Type == "UserName");
                if (usernameClaim != null)
                {
                    UserNameLoginCurrent = usernameClaim.Value;
                }

                return new AuthenticationState(principal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                await _localStorage.RemoveItemAsync("token");
                UserNameLoginCurrent = "User"; // Reset về default
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}