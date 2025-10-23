using LogisticsWebApp;
using Blazored.LocalStorage;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using LogisticsWebApp.Helper;
using logistic_web.application.Helpers;
var builder = WebApplication.CreateBuilder(args);

//Service của blazor server app
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

//add service http client - Tự động đọc từ appsettings theo môi trường
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7000";
Console.WriteLine($"🔗 API Base URL: {apiBaseUrl}"); // Debug log

builder.Services.AddHttpClient("LogisticApi", client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl}/api/");
});

builder.Services.AddBlazoredLocalStorage(); //lưu trữ local
builder.Services.AddSweetAlert2(); // SweetAlert2 service




// Đăng ký JWT Service
builder.Services.AddScoped<JwtAuthService>();

//Thêm middleware authentication
var privateKey = builder.Configuration["Jwt:SecretKey"];
var Issuer = builder.Configuration["Jwt:Issuer"];
var Audience = builder.Configuration["Jwt:Audience"];

// Thêm dịch vụ Authentication vào ứng dụng, sử dụng JWT Bearer làm phương thức xác thực
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    // Thiết lập các tham số xác thực token
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        // Kiểm tra và xác nhận Issuer (nguồn phát hành token)
        ValidateIssuer = true,
        ValidIssuer = Issuer, // Biến `Issuer` chứa giá trị của Issuer hợp lệ
                              // Kiểm tra và xác nhận Audience (đối tượng nhận token)
        ValidateAudience = true,
        ValidAudience = Audience, // Biến `Audience` chứa giá trị của Audience hợp lệ
                                  // Kiểm tra và xác nhận khóa bí mật được sử dụng để ký token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)),
        // Sử dụng khóa bí mật (`privateKey`) để tạo SymmetricSecurityKey nhằm xác thực chữ ký của token
        // Giảm độ trễ (skew time) của token xuống 0, đảm bảo token hết hạn chính xác
        ClockSkew = TimeSpan.Zero,
        // Xác định claim chứa vai trò của user (để phân quyền)
        RoleClaimType = ClaimTypes.Role,
        // Xác định claim chứa tên của user
        NameClaimType = ClaimTypes.Name,
        // Kiểm tra thời gian hết hạn của token, không cho phép sử dụng token hết hạn
        ValidateLifetime = true
    };
});

// Thêm dịch vụ Authorization để hỗ trợ phân quyền người dùng
builder.Services.AddAuthorization();

//Custom phân quyền blazor page
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();



var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
//Cấu hình các tệp tỉnh 
app.UseStaticFiles();


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


//authentication 
app.UseAuthentication(); // yêu cầu verify token
app.UseAuthorization(); // yêu cầu verify roles của token

app.Run();