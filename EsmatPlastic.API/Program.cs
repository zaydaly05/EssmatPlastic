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
// NEON DATABASE CONNECTION
// ============================================================

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    throw new InvalidOperationException(
        "DATABASE_URL environment variable is not set.");
}

var neonConnectionString = ConvertNeonUrlToConnectionString(databaseUrl);

Console.WriteLine("========================================");
Console.WriteLine("DATABASE: NEON POSTGRESQL");
Console.WriteLine("HOST: " + GetSafeHost(databaseUrl));
Console.WriteLine("========================================");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(neonConnectionString));

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
// CREATE NEON DATABASE SCHEMA
// ============================================================

Console.WriteLine("");
Console.WriteLine("Checking Neon database schema...");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("Creating Neon database schema...");

        db.Database.EnsureCreated();

        Console.WriteLine("Neon database tables created successfully.");

        Console.WriteLine("Neon database schema is ready.");
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

// ============================================================
// HELPERS
// ============================================================

static string ConvertNeonUrlToConnectionString(string databaseUrl)
{
    var value = databaseUrl.Trim().Trim('"').Trim('\'');

    if (value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        value = value.Substring("postgresql://".Length);
    }
    else if (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        value = value.Substring("postgres://".Length);
    }
    else
    {
        throw new InvalidOperationException(
            "DATABASE_URL must start with postgresql:// or postgres://.");
    }

    var queryIndex = value.IndexOf('?');

    if (queryIndex >= 0)
    {
        value = value.Substring(0, queryIndex);
    }

    var slashIndex = value.IndexOf('/');

    if (slashIndex < 0)
    {
        throw new InvalidOperationException(
            "DATABASE_URL is missing the database name.");
    }

    var authority = value.Substring(0, slashIndex);
    var database = value.Substring(slashIndex + 1);

    var atIndex = authority.LastIndexOf('@');

    if (atIndex < 0)
    {
        throw new InvalidOperationException(
            "DATABASE_URL is missing the username/password section.");
    }

    var userInfo = authority.Substring(0, atIndex);
    var hostPort = authority.Substring(atIndex + 1);

    var colonIndex = userInfo.IndexOf(':');

    if (colonIndex < 0)
    {
        throw new InvalidOperationException(
            "DATABASE_URL is missing the password.");
    }

    var username = Uri.UnescapeDataString(
        userInfo.Substring(0, colonIndex));

    var password = Uri.UnescapeDataString(
        userInfo.Substring(colonIndex + 1));

    var host = hostPort;
    var port = 5432;

    var lastColon = hostPort.LastIndexOf(':');

    if (lastColon > 0 &&
        int.TryParse(hostPort.Substring(lastColon + 1), out var parsedPort))
    {
        host = hostPort.Substring(0, lastColon);
        port = parsedPort;
    }

    database = Uri.UnescapeDataString(database);

    return
        $"Host={host};" +
        $"Port={port};" +
        $"Database={database};" +
        $"Username={username};" +
        $"Password={password};" +
        "SSL Mode=Require;" +
        "Trust Server Certificate=true;";
}

static string GetSafeHost(string databaseUrl)
{
    try
    {
        var uri = new Uri(databaseUrl);
        return uri.Host;
    }
    catch
    {
        return "Neon";
    }
}



