using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Entities.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IISIntegration;
using EngAce.Api;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration - Support both SQL Server and MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseProvider = builder.Configuration["DatabaseProvider"];

if (databaseProvider?.ToLower() == "mysql")
{
    connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
            b => b.MigrationsAssembly("EngAce.Api"))
    );
}
else
{
    // Default to SQL Server
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("EngAce.Api"))
    );
}

// Add controllers with JSON options (keep original property names)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Configure form options for file upload
builder.Services.Configure<FormOptions>(options =>
{
    // Set maximum file size to 100MB
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

// Configure Kestrel server limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

// Application telemetry and helpers
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();

// Register Services
builder.Services.AddScoped<EngAce.Api.Services.Interfaces.IReadingExerciseService, EngAce.Api.Services.ReadingExerciseService>();
builder.Services.AddScoped<EngAce.Api.Services.Interfaces.IProgressService, EngAce.Api.Services.ProgressService>();
builder.Services.AddScoped<EngAce.Api.Services.Interfaces.ILeaderboardService, EngAce.Api.Services.LeaderboardService>();
builder.Services.AddScoped<EngAce.Api.Services.Interfaces.IGeminiService, EngAce.Api.Services.AI.GeminiService>();

// Register HttpClient for Gemini service
builder.Services.AddHttpClient<EngAce.Api.Services.AI.GeminiService>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EngBuddy APIs Documentation",
        Version = "v1.0.0",
        Description = "EngAce / EngBuddy API"
    });

    // Simple API key security definition (adjust as needed)
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authentication",
        Type = SecuritySchemeType.ApiKey,
        Description = "The API Key or the Access Token to access protected endpoints"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if present
    try
    {
        string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
    }
    catch { /* ignore if not available */ }
});

// Response compression
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// CORS: a permissive dev policy and a stricter production policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });

    options.AddPolicy("AllowOnlyEngace", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173", 
                "https://localhost:3000",
                "https://localhost:5173",
                "http://localhost:8081",
                "https://localhost:8081",
                "https://your-production-domain.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure HttpContextHelper after app is built
var httpAccessor = app.Services.GetService<IHttpContextAccessor>();
if (httpAccessor is not null)
{
    HttpContextHelper.Configure(httpAccessor);
}

// Developer-friendly error page
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EngBuddy APIs Documentation v1.0.0");
        c.RoutePrefix = "swagger";
    });

    // Allow all origins in development for convenience
    app.UseCors("AllowAll");
}
else
{
    // Production: basic origin check (customize as needed)
    app.Use(async (context, next) =>
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Access Denied: Origin required.");
            return;
        }

        await next();
    });

    app.UseCors("AllowOnlyEngace");
}

// Global exception handler - log all unhandled exceptions
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "=== UNHANDLED EXCEPTION === Path: {Path}, Method: {Method}, Message: {Message}", 
            context.Request.Path, context.Request.Method, ex.Message);
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = $"Internal server error: {ex.Message}" });
    }
});

// Request logging middleware - log ALL requests before routing
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("=== INCOMING REQUEST === {Method} {Path} | ContentType: {ContentType} | ContentLength: {ContentLength}", 
        context.Request.Method, 
        context.Request.Path, 
        context.Request.ContentType ?? "NULL",
        context.Request.ContentLength?.ToString() ?? "NULL");
    
    try
    {
        await next();
        logger.LogInformation("=== REQUEST COMPLETED === {Method} {Path} | StatusCode: {StatusCode}", 
            context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "=== REQUEST FAILED === {Method} {Path} | Exception: {Message}", 
            context.Request.Method, context.Request.Path, ex.Message);
        throw;
    }
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
