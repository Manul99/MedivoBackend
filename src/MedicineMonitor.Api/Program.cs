using System.Text.Json.Serialization;
using MedicineMonitor.Api.Authentication;
using MedicineMonitor.Api.Extensions;
using MedicineMonitor.Api.Middleware;
using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.Services;
using MedicineMonitor.Infrastructure;
using MedicineMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var postgresConnection =
    builder.Configuration.GetConnectionString("PostgreSQL");

Console.WriteLine(
    $"PostgreSQL configured: {!string.IsNullOrWhiteSpace(postgresConnection)}");

Console.WriteLine(
    $"PostgreSQL host info: {postgresConnection?.Split('@').LastOrDefault()}");

builder.Services.AddSingleton(NpgsqlDataSource.Create(postgresConnection));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MedicationService>();
builder.Services.AddHttpClient();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "FirebaseSession";
        options.DefaultChallengeScheme = "FirebaseSession";
    })
    .AddScheme<AuthenticationSchemeOptions, FirebaseSessionAuthenticationHandler>(
        "FirebaseSession", _ => { });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("WebClient");
app.UseMiddleware<CsrfMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue("Database:ApplyEnsureCreatedOnStartup", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MedicineMonitorDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();

public partial class Program { }
