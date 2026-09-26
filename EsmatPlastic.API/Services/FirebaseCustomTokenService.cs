using EsmatPlastic.Domain.Entities;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace EsmatPlastic.API.Services;

public sealed class FirebaseCustomTokenService : IFirebaseCustomTokenService
{
    private readonly IConfiguration _configuration;
    private readonly object _sync = new();
    private FirebaseAuth? _firebaseAuth;

    public FirebaseCustomTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> CreateAsync(
        User user,
        IReadOnlyCollection<string> permissions)
    {
        var projectId = _configuration["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new InvalidOperationException("Firebase:ProjectId is not configured.");
        }

        var firebaseAuth = GetFirebaseAuth(projectId);
        var uid = user.SyncId == Guid.Empty
            ? user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : user.SyncId.ToString("D");

        var claims = new Dictionary<string, object>
        {
            ["appRole"] = user.Role.ToString(),
            ["permissions"] = permissions.ToArray()
        };

        return await firebaseAuth.CreateCustomTokenAsync(uid, claims);
    }

    private FirebaseAuth GetFirebaseAuth(string projectId)
    {
        if (_firebaseAuth is not null)
        {
            return _firebaseAuth;
        }

        lock (_sync)
        {
            if (_firebaseAuth is not null)
            {
                return _firebaseAuth;
            }

            var app = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.GetApplicationDefault(),
                ProjectId = projectId
            }, "EsmatPlastic");

            _firebaseAuth = FirebaseAuth.GetAuth(app);
            return _firebaseAuth;
        }
    }
}