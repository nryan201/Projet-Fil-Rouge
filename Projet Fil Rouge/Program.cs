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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:8080") // ton front !
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // obligatoire pour envoyer les cookies
    });
});

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
builder.Services.AddHttpClient("RteApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApi:BaseUrl"]!);
});

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
app.UseCors("AllowFrontend");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }