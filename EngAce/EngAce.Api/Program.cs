using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add controllers with JSON options (keep original property names)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Application telemetry and helpers (if Helper project is available)
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpContextAccessor();
// Configure a HttpContext helper if available
var sp = builder.Services.BuildServiceProvider();
var httpAccessor = sp.GetService<IHttpContextAccessor>();
if (httpAccessor is not null)
{
    HttpContextHelper.Configure(httpAccessor);
}

builder.Services.AddMemoryCache();

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
