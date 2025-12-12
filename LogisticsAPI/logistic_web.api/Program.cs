using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.Services;
using logistic_web.application.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


// service EF
var connectionString = builder.Configuration.GetConnectionString("connectionStringLogistic");
builder.Services.AddDbContext<LogisticContext>(options =>
    options.UseSqlServer(connectionString));

// Response caching cache
builder.Services.AddResponseCaching();

//sử dụng map controller
builder.Services.AddControllers();

//Swagger cấu hình có điền Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Logistics API", Version = "v1" });

    // 🔥 Thêm hỗ trợ Authorization header tất cả api
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token vào ô bên dưới theo định dạng: Bearer {token}"
    });

    // 🔥 Định nghĩa yêu cầu sử dụng Authorization trên từng api
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//Di Infrastructure (Repositories + UnitOfWork)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICargolistRepository, CargolistRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IShipperRepository, ShipperRepository>();
builder.Services.AddScoped<ITrackingRepository, TrackingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Di Application (Services)
builder.Services.AddScoped<ICargoService, CargoService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IShipperService, ShipperService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<JwtAuthService>();

//Cài đặt Jwt Bearer Authentication
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

//add service cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin() // Allow all origins for Aspire service discovery
         .AllowAnyHeader()
         .AllowAnyMethod();
    });
});

var app = builder.Build();


app.UseHttpsRedirection();
app.UseResponseCaching();

// Enable CORS
app.UseCors("AllowAll");

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//use swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();