using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using System.Reflection;
using EngAce.Api.Services;
using EngAce.Api.Repositories;
using EngAce.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging to Console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpContextAccessor();
var serviceProvider = builder.Services.BuildServiceProvider();
HttpContextHelper.Configure(
    serviceProvider.GetRequiredService<IHttpContextAccessor>(),
    serviceProvider.GetRequiredService<IConfiguration>()
);
builder.Services.AddMemoryCache();

// Register Authentication Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EngBuddy APIs Documentation",
        Version = "v1.0.0",
        Description = "Developed by ."
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authentication",
        Type = SecuritySchemeType.ApiKey,
        Description = "The API Key or the Access Token to access Gemini services"
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

    c.UseAllOfToExtendReferenceSchemas();

    c.MapType<ProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProblemDetails" } });
    c.MapType<ValidationProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ValidationProblemDetails" } });
    c.MapType<SerializableError>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "SerializableError" } });

    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOnlyEngBuddy",
        policy =>
        {
            policy.WithOrigins()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    
    // Add CORS policy for Frontend
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://localhost:5173", "http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

builder.Services.AddResponseCaching();

var app = builder.Build();

// Log all requests
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("==================== NEW REQUEST ====================");
    logger.LogInformation($"📥 {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    logger.LogInformation($"🌐 From: {context.Connection.RemoteIpAddress}");
    logger.LogInformation($"📋 Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
    
    await next();
    
    logger.LogInformation($"📤 Response: {context.Response.StatusCode}");
    logger.LogInformation("====================================================");
});

app.UseDeveloperExceptionPage();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

// Add JWT Middleware
app.UseMiddleware<JwtMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var origin = context.Request.Headers.Origin.ToString();

        if (string.IsNullOrEmpty(origin))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Access Denied.");
            return;
        }

        await next();
    });

    app.UseCors("AllowOnlyEngace");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EngBuddy APIs Documentation v1.0.0");
        c.RoutePrefix = "swagger";
    });

    // Allow Frontend in Development
    app.UseCors("AllowFrontend");
}

app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
