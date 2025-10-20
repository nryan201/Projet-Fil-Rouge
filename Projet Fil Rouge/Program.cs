using Microsoft.EntityFrameworkCore;
using Projet_Fil_Rouge;
using Projet_Fil_Rouge.BLL;
using System;
using System.Xml.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services
    .AddHealthChecks()
    .AddSqlServer(connectionString);
builder.Services.AddScoped<CredentialBLL>();

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
