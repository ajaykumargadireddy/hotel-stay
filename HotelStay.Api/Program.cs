using HotelStay.Api.Endpoints;
using HotelStay.Api.Extensions;
using HotelStay.Api.Middleware;
using HotelStay.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Microsoft.Extensions.Logging with console + rolling file logging (Karambolo)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.AddLoggingProvider();

builder.Services.AddProblemDetails();
builder.Services.AddServices();

// Configure JSON serialization to handle string enum values
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Enable CORS for Angular UI (development)
builder.Services.AddCorsPolicy(builder.Configuration);

// Add Swagger/OpenAPI with detailed configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Hotel Stay API");

// Configure Swagger (available in all environments for testing)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Stay API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root URL
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AngularUi");

// Map hotel endpoints
app.MapHotelEndpoints();
app.MapLookupEndpoints();

logger.LogInformation("Hotel Stay API started successfully");

app.Run();

