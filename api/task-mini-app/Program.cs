using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Connection string
var connectionString = builder.Configuration.GetConnectionString("Default");

// ✅ DbContext kaydı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// ✅ Service kayıtları
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>(); // varsa

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();