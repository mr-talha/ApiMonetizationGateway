using ApiMonetizationGateway.BackgroundServices;
using ApiMonetizationGateway.Data;
using ApiMonetizationGateway.Middlewares;
using ApiMonetizationGateway.Redis;
using ApiMonetizationGateway.Redis.Service;
using ApiMonetizationGateway.Redis.Service.Impl;
using ApiMonetizationGateway.Services.CustomerService;
using ApiMonetizationGateway.Services.CustomerService.Impl;
using ApiMonetizationGateway.Services.RateLimitingService;
using ApiMonetizationGateway.Services.RateLimitingService.Impl;
using ApiMonetizationGateway.Services.UsageTrackingService;
using ApiMonetizationGateway.Services.UsageTrackingService.Impl;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

#nullable disable

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// Database
var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(sqlConnectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
    }));

// Register services
builder.Services.AddScoped<IRateLimitingStorage, RedisRateLimitingStorage>();
builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUsageTrackingService, UsageTrackingService>();

// Background services
builder.Services.AddHostedService<MonthlySummaryService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConnectionString)
    //.AddRedis(redisConnectionString)
    .AddCheck<RedisHealthCheck>("redis"); // use custom Redis health check


// Register RedisHealthCheck in DI
builder.Services.AddSingleton<RedisHealthCheck>();

var app = builder.Build();

// Swagger in Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Rate limiting before auth
app.UseMiddleware<RateLimitingMiddleware>();

// Log API usage
app.UseMiddleware<ApiUsageLoggingMiddleware>();

app.UseAuthorization();

// Health check endpoint with JSON output
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                }),
                duration = report.TotalDuration.TotalMilliseconds
            });
        await context.Response.WriteAsync(result);
    }
});

// Database migration + seeding
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");

        //await SeedData.InitializeAsync(db);
        //logger.LogInformation("Database seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

app.MapControllers();

await app.RunAsync();

public partial class Program { }
