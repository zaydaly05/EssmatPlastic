using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EsmatPlastic.Shared.Services;

public sealed class FirebaseAuthClient
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> ExchangeCustomTokenAsync(string customToken, string webApiKey)
    {
        if (string.IsNullOrWhiteSpace(customToken) || string.IsNullOrWhiteSpace(webApiKey))
        {
            throw new InvalidOperationException("Firebase sign-in is not configured.");
        }

        var endpoint = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithCustomToken?key=" +
            Uri.EscapeDataString(webApiKey);
        using var response = await HttpClient.PostAsJsonAsync(endpoint, new
        {
            token = customToken,
            returnSecureToken = true
        });

        var responseBody = await response.Content.ReadAsStringAsync();
        FirebaseTokenResponse? result = null;
        try
        {
            result = JsonSerializer.Deserialize<FirebaseTokenResponse>(responseBody, JsonOptions);
        }
        catch (JsonException)
        {
        }

        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(result?.IdToken))
        {
            throw new HttpRequestException(
                $"Firebase sign-in failed: {responseBody}",
                null,
                response.StatusCode);
        }

        return result.IdToken;
    }

    private sealed class FirebaseTokenResponse
    {
        public string IdToken { get; set; } = string.Empty;
    }
}

public sealed class FirebaseFirestoreClient
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _projectId;
    private string? _idToken;

    public FirebaseFirestoreClient(string projectId)
    {
        _projectId = projectId;
    }

    public void SetIdToken(string idToken) => _idToken = idToken;

    public void ClearIdToken() => _idToken = null;

    public string? AuthenticatedUserId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_idToken)) return null;
            try
            {
                var parts = _idToken.Split('.');
                if (parts.Length < 2) return null;
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
                return document.RootElement.TryGetProperty("sub", out var subject) ? subject.GetString() : null;
            }
            catch (FormatException) { return null; }
            catch (JsonException) { return null; }
        }
    }

    public async Task<IReadOnlyList<FirestoreDataDocument<T>>> GetCollectionAsync<T>(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(_idToken))
        {
            throw new InvalidOperationException("Sign in to Firebase before reading Firestore.");
        }

        var results = new List<FirestoreDataDocument<T>>();
        string? pageToken = null;
        do
        {
            var uri = new UriBuilder(
                $"https://firestore.googleapis.com/v1/projects/{Uri.EscapeDataString(_projectId)}/databases/(default)/documents/{Uri.EscapeDataString(collectionName)}")
            {
                Query = pageToken is null ? "pageSize=500" : "pageSize=500&pageToken=" + Uri.EscapeDataString(pageToken)
            }.Uri;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
            using var response = await HttpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Firestore read failed for '{collectionName}': {responseBody}",
                    null,
                    response.StatusCode);
            }

            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("documents", out var documents))
            {
                foreach (var cloudDocument in documents.EnumerateArray())
                {
                    var fields = cloudDocument.GetProperty("fields");
                    var payloadJson = fields.GetProperty("payloadJson").GetProperty("stringValue").GetString();
                    if (string.IsNullOrWhiteSpace(payloadJson))
                    {
                        continue;
                    }

                    var payload = JsonSerializer.Deserialize<T>(payloadJson, JsonOptions);
                    if (payload is null)
                    {
                        continue;
                    }

                    var references = ReadReferences(fields);
                    var documentName = cloudDocument.GetProperty("name").GetString() ?? string.Empty;
                    results.Add(new FirestoreDataDocument<T>(payload, references, documentName.Split('/').Last()));
                }
            }

            pageToken = document.RootElement.TryGetProperty("nextPageToken", out var nextPageToken)
                ? nextPageToken.GetString()
                : null;
        }
        while (!string.IsNullOrWhiteSpace(pageToken));

        return results;
    }

    public async Task<FirestoreDataDocument<T>> GetDocumentAsync<T>(string collectionName, string documentId)
    {
        if (string.IsNullOrWhiteSpace(_idToken))
            throw new InvalidOperationException("Sign in to Firebase before reading Firestore.");
        var uri = $"https://firestore.googleapis.com/v1/projects/{Uri.EscapeDataString(_projectId)}/databases/(default)/documents/{Uri.EscapeDataString(collectionName)}/{Uri.EscapeDataString(documentId)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
        using var response = await HttpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Firestore read failed for '{collectionName}/{documentId}': {responseBody}", null, response.StatusCode);
        using var document = JsonDocument.Parse(responseBody);
        var fields = document.RootElement.GetProperty("fields");
        var payloadJson = fields.GetProperty("payloadJson").GetProperty("stringValue").GetString();
        var payload = string.IsNullOrWhiteSpace(payloadJson) ? default : JsonSerializer.Deserialize<T>(payloadJson, JsonOptions);
        if (payload is null) throw new InvalidOperationException($"Firestore document '{collectionName}/{documentId}' has no valid payload.");
        return new FirestoreDataDocument<T>(payload, ReadReferences(fields), documentId);
    }

    public async Task WriteDocumentAsync<T>(string collectionName, string documentId, T payload,
        IReadOnlyDictionary<string, string>? references = null)
    {
        if (string.IsNullOrWhiteSpace(_idToken))
            throw new InvalidOperationException("Sign in to Firebase before writing to Firestore.");

        var now = DateTime.UtcNow;
        var syncId = Guid.TryParse(documentId, out var parsed) ? parsed : Guid.NewGuid();
        var body = JsonSerializer.Serialize(new
        {
            fields = new Dictionary<string, object>
            {
                ["syncId"] = new { stringValue = syncId.ToString("D") },
                ["updatedAt"] = new { timestampValue = now.ToString("O") },
                ["payloadJson"] = new { stringValue = JsonSerializer.Serialize(payload, JsonOptions) },
                ["references"] = new { mapValue = new { fields = (references ?? new Dictionary<string, string>()).ToDictionary(
                    item => item.Key, item => (object)new { stringValue = item.Value }) } }
            }
        }, JsonOptions);

        var uri = $"https://firestore.googleapis.com/v1/projects/{Uri.EscapeDataString(_projectId)}/databases/(default)/documents/{Uri.EscapeDataString(collectionName)}/{Uri.EscapeDataString(syncId.ToString("D"))}";
        using var request = new HttpRequestMessage(HttpMethod.Patch, uri)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
        using var response = await HttpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Firestore write failed for '{collectionName}': {responseBody}", null, response.StatusCode);
    }

    public Task WriteTombstoneAsync(string entityType, string recordKey) =>
        WriteRawFieldsAsync("deletedRecords", Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{entityType}|{recordKey}"))).ToLowerInvariant(), new Dictionary<string, object>
        {
            ["entityType"] = new { stringValue = entityType },
            ["recordKey"] = new { stringValue = recordKey },
            ["deletedAt"] = new { timestampValue = DateTime.UtcNow.ToString("O") }
        });

    public Task WriteUserCredentialAsync(string userSyncId, string passwordHash) =>
        WriteRawFieldsAsync("userCredentials", userSyncId, new Dictionary<string, object>
        {
            ["passwordHash"] = new { stringValue = passwordHash },
            ["updatedAt"] = new { timestampValue = DateTime.UtcNow.ToString("O") }
        });

    public async Task DeleteDocumentAsync(string collectionName, string documentId)
    {
        if (string.IsNullOrWhiteSpace(_idToken))
            throw new InvalidOperationException("Sign in to Firebase before writing to Firestore.");
        var uri = $"https://firestore.googleapis.com/v1/projects/{Uri.EscapeDataString(_projectId)}/databases/(default)/documents/{Uri.EscapeDataString(collectionName)}/{Uri.EscapeDataString(documentId)}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
        using var response = await HttpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            throw new HttpRequestException($"Firestore delete failed for '{collectionName}': {responseBody}", null, response.StatusCode);
    }

    private async Task WriteRawFieldsAsync(string collectionName, string documentId, IReadOnlyDictionary<string, object> fields)
    {
        if (string.IsNullOrWhiteSpace(_idToken))
            throw new InvalidOperationException("Sign in to Firebase before writing to Firestore.");
        var body = JsonSerializer.Serialize(new { fields }, JsonOptions);
        var uri = $"https://firestore.googleapis.com/v1/projects/{Uri.EscapeDataString(_projectId)}/databases/(default)/documents/{Uri.EscapeDataString(collectionName)}/{Uri.EscapeDataString(documentId)}";
        using var request = new HttpRequestMessage(HttpMethod.Patch, uri)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
        using var response = await HttpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Firestore write failed for '{collectionName}': {responseBody}", null, response.StatusCode);
    }

    private static Dictionary<string, string> ReadReferences(JsonElement fields)
    {
        var references = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!fields.TryGetProperty("references", out var referenceValue) ||
            !referenceValue.TryGetProperty("mapValue", out var mapValue) ||
            !mapValue.TryGetProperty("fields", out var values))
        {
            return references;
        }

        foreach (var property in values.EnumerateObject())
        {
            if (property.Value.TryGetProperty("stringValue", out var value))
            {
                references[property.Name] = value.GetString() ?? string.Empty;
            }
        }

        return references;
    }
}

public sealed record FirestoreDataDocument<T>(T Data, IReadOnlyDictionary<string, string> References, string SyncId = "");
