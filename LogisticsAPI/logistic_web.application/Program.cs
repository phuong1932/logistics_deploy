using logistic_web.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("connectionStringLogistic");
builder.Services.AddDbContext<LogisticContext>(options =>
    options.UseSqlServer(connectionString));


//DI các repository để dùng trong các lớp service
// builder.Services.AddScoped<IRepository, Repository>();




var app = builder.Build();

app.Run();
