using System.Text;
using EsmatPlastic.API;
using EsmatPlastic.API.Authorization;
using EsmatPlastic.API.Services;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EsmatPlastic.API",
        Version = "1.0"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================
// DATABASE CONNECTION
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// ============================================================
// JWT
// ============================================================

var jwtSettings = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");

var jwtIssuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is not configured.");

var jwtAudience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("JWT Audience is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Stock.View", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Stock.View")));

    options.AddPolicy("Products.View", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Products.View")));

    options.AddPolicy("Products.Create", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Products.Create")));

    options.AddPolicy("Products.Edit", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Products.Edit")));

    options.AddPolicy("Products.Delete", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Products.Delete")));

    options.AddPolicy("Reports.View", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Reports.View")));

    options.AddPolicy("Users.View", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Users.View")));

    options.AddPolicy("Users.Create", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Users.Create")));

    options.AddPolicy("Users.Edit", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Users.Edit")));

    options.AddPolicy("Users.Delete", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Users.Delete")));

    options.AddPolicy("Permissions.Manage", policy =>
        policy.Requirements.Add(
            new PermissionRequirement("Permissions.Manage")));
});

builder.Services.AddSingleton<
    Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    PermissionAuthorizationHandler>();

// ============================================================
// SERVICES
// ============================================================

builder.Services.AddSingleton<IDbConnectionManager>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new DbConnectionManager(
        config.GetConnectionString("NeonConnection") ?? string.Empty,
        config.GetConnectionString("DefaultConnection") ?? string.Empty,
        sp.GetRequiredService<ILogger<DbConnectionManager>>()
    );
});
builder.Services.AddSingleton<IDbSyncTrigger, DbSyncBackgroundService>();
builder.Services.AddHostedService(sp => (DbSyncBackgroundService)sp.GetRequiredService<IDbSyncTrigger>());
builder.Services.AddScoped<IResilientDbExecutor, ResilientDbExecutor>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<
    IProductVariantService,
    ProductVariantService>();
builder.Services.AddScoped<IStockService, StockService>();

// ============================================================
// BUILD APP
// ============================================================

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception =
            context.Features
                .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()
                ?.Error;

        app.Logger.LogError(
            exception,
            "Unhandled API exception for {Path}",
            context.Request.Path);

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        context.Response.ContentType =
            "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(new
        {
            message = "حدث خطأ داخلي في الخادم. يرجى المحاولة مرة أخرى.",
            status = context.Response.StatusCode
        });
    });
});

// ============================================================
// LOCAL DATABASE SCHEMA
// ============================================================

Console.WriteLine("");
Console.WriteLine("Checking local database schema...");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("Ensuring local database schema is created...");

        db.Database.EnsureCreated();

        Console.WriteLine("Local database tables ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("");
        Console.WriteLine("DATABASE CONNECTION FAILED.");
        Console.WriteLine(ex.Message);
        Console.WriteLine("");
        throw;
    }
}

// ============================================================
// HTTP PIPELINE
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================
// SEED DATA
// ============================================================

if (app.Environment.IsDevelopment())
{
    await DatabaseSeeder.SeedAsync(app.Services);
    await TestDataSeeder.SeedAsync(app.Services);
}

Console.WriteLine("");
Console.WriteLine("========================================");
Console.WriteLine(" ESMAT PLASTIC API");
Console.WriteLine(" DATABASE: NEON");
Console.WriteLine(" API STARTING...");
Console.WriteLine("========================================");
Console.WriteLine("");

app.Run();



