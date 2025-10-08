using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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
HttpContextHelper.Configure(builder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>());
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EngBuddy APIs Documentation",
        Version = "v1.0.0",
        Description = "Developed by Nhóm 12."
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

<<<<<<< HEAD
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
=======
// ✅ CORS cấu hình
builder.Services.AddCors(options =>
{
    // Policy cho phép tất cả
    options.AddPolicy("AllowAll", policy =>
>>>>>>> 9cd833f (Fix merge Program.cs before rebase)
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
<<<<<<< HEAD
=======

    // Policy chỉ cho phép origin EngAce (bạn cần thêm domain cụ thể vào WithOrigins)
    options.AddPolicy("AllowOnlyEngace", policy =>
    {
        policy.WithOrigins("http://your-fe-domain.com") // TODO: thay domain FE thật vào
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
>>>>>>> 9cd833f (Fix merge Program.cs before rebase)
});

builder.Services.AddResponseCaching();

var app = builder.Build();

app.UseDeveloperExceptionPage();
<<<<<<< HEAD
=======

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
>>>>>>> 9cd833f (Fix merge Program.cs before rebase)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EngBuddy APIs Documentation v1.0.0");
        c.RoutePrefix = "swagger";
    });

    // Dev thì cho phép tất cả origin
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseRouting();
<<<<<<< HEAD

app.UseCors();
=======
>>>>>>> 9cd833f (Fix merge Program.cs before rebase)
app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
