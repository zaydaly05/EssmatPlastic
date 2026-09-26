using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EsmatPlastic.API.DTOs.Auth;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EsmatPlastic.API.Services;

public class AuthService : IAuthService
{
    private readonly IResilientDbExecutor _executor;
    private readonly IConfiguration _configuration;
    private readonly IFirebaseCustomTokenService _firebaseCustomTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        IResilientDbExecutor executor,
        IConfiguration configuration,
        IFirebaseCustomTokenService firebaseCustomTokenService,
        ILogger<AuthService> logger)
    {
        _executor = executor;
        _configuration = configuration;
        _firebaseCustomTokenService = firebaseCustomTokenService;
        _logger = logger;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var username = request.Username.Trim();

        return await _executor.ExecuteAsync(async db =>
        {
            var user = await db.Users
                .Include(x => x.UserPermissions)
                .ThenInclude(x => x.Permission)
                .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);

            if (user is null) return null;

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordResult == PasswordVerificationResult.Failed) return null;

            var permissions = user.UserPermissions
                .Where(x => x.Permission.IsActive)
                .Select(x => x.Permission.Name)
                .ToList();

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");

            var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var minutes) ? minutes : 480;
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("FullName", user.FullName)
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            string? firebaseCustomToken = null;
            try
            {
                firebaseCustomToken = await _firebaseCustomTokenService.CreateAsync(user, permissions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase sign-in token could not be issued; API login remains available.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Permissions = permissions,
                FirebaseCustomToken = firebaseCustomToken,
                FirebaseWebApiKey = _configuration["Firebase:WebApiKey"]
            };
        });
    }
}
