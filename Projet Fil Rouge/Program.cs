using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projet_Fil_Rouge;
using Projet_Fil_Rouge.Auth;
using Projet_Fil_Rouge.BLL;
using Projet_Fil_Rouge.Entities;



var builder = WebApplication.CreateBuilder(args);
Env.Load();

var jwtKey = Environment.GetEnvironmentVariable("JwtKey") ?? throw new InvalidOperationException("JwtKey manquant");

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services
    .AddHealthChecks()
    .AddSqlServer(connectionString);
builder.Services.AddScoped<CredentialBLL>();
builder.Services.AddScoped<IPasswordHasher<Credential>, PasswordHasher<Credential>>();
builder.Services.AddControllers();
builder.Services.AddJwtAuth(jwtKey);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHealthChecks("/health/db");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

