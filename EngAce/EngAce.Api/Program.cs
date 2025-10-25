using Helper;using Helper;

using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.ResponseCompression;using Microsoft.AspNetCore.ResponseCompression;

using Microsoft.OpenApi.Models;using Microsoft.OpenApi.Models;

using System.Reflection;using System.Reflection;



var builder = WebApplication.CreateBuilder(args);var builder = WebApplication.CreateBuilder(args);



if (builder.Environment.IsDevelopment())if (builder.Environment.IsDevelopment())

{{

    builder.Services.AddEndpointsApiExplorer();    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen();    builder.Services.AddSwaggerGen();

}}



builder.Services.AddControllers()builder.Services.AddControllers()

    .AddJsonOptions(options =>    .AddJsonOptions(options =>

    {    {

        options.JsonSerializerOptions.PropertyNamingPolicy = null;        options.JsonSerializerOptions.PropertyNamingPolicy = null;

    });    });



builder.Services.AddApplicationInsightsTelemetry();builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddHttpContextAccessor();builder.Services.AddHttpContextAccessor();

HttpContextHelper.Configure(builder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>());HttpContextHelper.Configure(builder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>());

builder.Services.AddMemoryCache();builder.Services.AddMemoryCache();



builder.Services.AddEndpointsApiExplorer();builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>builder.Services.AddSwaggerGen(c =>

{{

    c.SwaggerDoc("v1", new OpenApiInfo    c.SwaggerDoc("v1", new OpenApiInfo

    {    {

        Title = "EngBuddy APIs Documentation",        Title = "EngBuddy APIs Documentation",

        Version = "v1.0.0",        Version = "v1.0.0",

        Description = "Developed by Nhóm 12."        Description = "Developed by Nhóm 12."

    });    });



    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme

    {    {

        In = ParameterLocation.Header,        In = ParameterLocation.Header,

        Name = "Authentication",        Name = "Authentication",

        Type = SecuritySchemeType.ApiKey,        Type = SecuritySchemeType.ApiKey,

        Description = "The API Key or the Access Token to access Gemini services"        Description = "The API Key or the Access Token to access Gemini services"

    });    });



    c.AddSecurityRequirement(new OpenApiSecurityRequirement    c.AddSecurityRequirement(new OpenApiSecurityRequirement

    {    {

        {        {

            new OpenApiSecurityScheme            new OpenApiSecurityScheme

            {            {

                Reference = new OpenApiReference                Reference = new OpenApiReference

                {                {

                    Type = ReferenceType.SecurityScheme,                    Type = ReferenceType.SecurityScheme,

                    Id = "ApiKey"                    Id = "ApiKey"

                }                }

            },            },

            Array.Empty<string>()            Array.Empty<string>()

        }        }

    });    });



    c.UseAllOfToExtendReferenceSchemas();    c.UseAllOfToExtendReferenceSchemas();



    c.MapType<ProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProblemDetails" } });    c.MapType<ProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProblemDetails" } });

    c.MapType<ValidationProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ValidationProblemDetails" } });    c.MapType<ValidationProblemDetails>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ValidationProblemDetails" } });

    c.MapType<SerializableError>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "SerializableError" } });            c.MapType<SerializableError>(() => new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "SerializableError" } });



    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);    c.IncludeXmlComments(xmlPath);

});});



builder.Services.AddResponseCompression(options =>builder.Services.AddResponseCompression(options =>

{{

    options.Providers.Add<GzipCompressionProvider>();    options.Providers.Add<GzipCompressionProvider>();

    options.Providers.Add<BrotliCompressionProvider>();    options.Providers.Add<BrotliCompressionProvider>();

});});



builder.Services.Configure<GzipCompressionProviderOptions>(options =>builder.Services.Configure<GzipCompressionProviderOptions>(options =>

{{

    options.Level = System.IO.Compression.CompressionLevel.Fastest;    options.Level = System.IO.Compression.CompressionLevel.Fastest;

});});



builder.Services.Configure<BrotliCompressionProviderOptions>(options =>builder.Services.Configure<BrotliCompressionProviderOptions>(options =>

{{

    options.Level = System.IO.Compression.CompressionLevel.Fastest;    options.Level = System.IO.Compression.CompressionLevel.Fastest;

});});



// 🌐 CORS configuration - Unified & Production Ready<<<<<<< HEAD

builder.Services.AddCors(options =>// ✅ CORS cấu hình

{builder.Services.AddCors(options =>

    // Policy cho development - Allow tất cả{

    options.AddPolicy("AllowAll", policy =>    // Policy cho phép tất cả

    {    options.AddPolicy("AllowAll", policy =>

        policy.AllowAnyOrigin()    {

              .AllowAnyHeader()        policy.AllowAnyOrigin()

              .AllowAnyMethod();              .AllowAnyHeader()

    });              .AllowAnyMethod();

    });

    // Policy cho production - Chỉ cho phép domain cụ thể

    options.AddPolicy("AllowOnlyEngace", policy =>    // Policy chỉ cho phép origin EngAce (bạn cần thêm domain cụ thể vào WithOrigins)

    {    options.AddPolicy("AllowOnlyEngace", policy =>

        policy.WithOrigins(    {

                "http://localhost:8081",           // Development frontend        policy.WithOrigins("http://your-fe-domain.com") // TODO: thay domain FE thật vào

                "https://localhost:8081",          // Development frontend HTTPS              .AllowAnyMethod()

                "https://your-production-domain.com" // TODO: Thay domain production thật              .AllowAnyHeader();

              )    });

              .AllowAnyMethod()=======

              .AllowAnyHeader()// ✅ CORS cấu hình cho phép tất cả origin

              .AllowCredentials(); // Cho phép credentials nếu cầnbuilder.Services.AddCors(options =>

    });{

});<<<<<<< HEAD

    options.AddDefaultPolicy(policyBuilder =>

builder.Services.AddResponseCaching();    {

        policyBuilder.AllowAnyOrigin()

var app = builder.Build();                     .AllowAnyHeader()

                     .AllowAnyMethod();

app.UseDeveloperExceptionPage();    });

=======

if (app.Environment.IsDevelopment())    options.AddPolicy("AllowOnlyEngace",

{        policy =>

    app.UseSwagger();        {

    app.UseSwaggerUI(c =>            policy.WithOrigins()

    {                  .AllowAnyMethod()

        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EngBuddy APIs Documentation v1.0.0");                  .AllowAnyHeader();

        c.RoutePrefix = "swagger";        });

    });});



    // Development: Allow tất cả originbuilder.Services.AddCors(options =>

    app.UseCors("AllowAll");{

}    options.AddPolicy("AllowAll",

else        policy =>

{        {

    // Production: Chỉ cho phép domain cụ thể và kiểm tra origin            policy.AllowAnyOrigin()

    app.Use(async (context, next) =>                  .AllowAnyMethod()

    {                  .AllowAnyHeader();

        var origin = context.Request.Headers.Origin.ToString();        });

        >>>>>>> 23b1174 (connect fe-be)

        if (string.IsNullOrEmpty(origin))>>>>>>> 3203149b1c1403f10e3e996daec8e54a0cc80bd9

        {});

            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            await context.Response.WriteAsync("Access Denied: Origin required.");builder.Services.AddResponseCaching();

            return;

        }var app = builder.Build();



        await next();app.UseDeveloperExceptionPage();

    });<<<<<<< HEAD

=======

    app.UseCors("AllowOnlyEngace");<<<<<<< HEAD

}=======

app.UseHttpsRedirection();

app.UseHttpsRedirection();app.UseRouting();

app.UseRouting();>>>>>>> 3203149b1c1403f10e3e996daec8e54a0cc80bd9

app.UseResponseCompression();

if (!app.Environment.IsDevelopment())

app.UseAuthentication();{

app.UseAuthorization();    app.Use(async (context, next) =>

    {

app.UseEndpoints(endpoints =>        var origin = context.Request.Headers.Origin.ToString();

{

    endpoints.MapControllers();        if (string.IsNullOrEmpty(origin))

});        {

            context.Response.StatusCode = StatusCodes.Status403Forbidden;

app.Run();            await context.Response.WriteAsync("Access Denied.");
            return;
        }

        await next();
    });

    app.UseCors("AllowOnlyEngace");
}
>>>>>>> 23b1174 (connect fe-be)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
<<<<<<< HEAD
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EngBuddy APIs Documentation v1.0.0");
        c.RoutePrefix = "swagger";
    });

    // Dev thì cho phép tất cả origin
    app.UseCors("AllowAll");
=======
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend APIs");
    });
>>>>>>> 3203149b1c1403f10e3e996daec8e54a0cc80bd9
}

app.UseHttpsRedirection();
app.UseRouting();
<<<<<<< HEAD
=======

// ✅ Dùng CORS đã cấu hình ở trên
app.UseCors();
>>>>>>> 3203149b1c1403f10e3e996daec8e54a0cc80bd9
app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
