using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EsmatPlastic.Desktop.Services.Settings;

namespace EsmatPlastic.Desktop.Services;

public class ApiClient
{
    private HttpClient _httpClient;
    private readonly SettingsService _settingsService;

    public ApiClient(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _httpClient = CreateHttpClient(settingsService.Current.ApiBaseUrl);
    }

    private static HttpClient CreateHttpClient(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://localhost:5023/";
        }

        if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
        {
            baseUrl = "http://" + baseUrl;
        }

        if (!baseUrl.EndsWith("/")) baseUrl += "/";

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            EnableMultipleHttp2Connections = true
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public void UpdateBaseUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        // Update the settings service first so it persists
        _settingsService.Current.ApiBaseUrl = url;
        _settingsService.Save();

        if (!url.EndsWith("/")) url += "/";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            if (_httpClient.BaseAddress == uri)
                return;

            var previousClient = _httpClient;
            var replacementClient = CreateHttpClient(url);
            replacementClient.DefaultRequestHeaders.Authorization =
                previousClient.DefaultRequestHeaders.Authorization;
            _httpClient = replacementClient;
            previousClient.Dispose();
        }
    }

    public async Task<HealthStatusResponse?> GetHealthStatusAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var response = await _httpClient.GetAsync("api/Health/status", cts.Token);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<HealthStatusResponse>(cancellationToken: cts.Token);
            }
        }
        catch
        {
            // API unreachable or connection timed out
        }

        return new HealthStatusResponse
        {
            Status = "Offline",
            Mode = "LocalNetworkPrimary",
            IsOnline = false,
            IsNeonBackupOnline = false,
            PrimaryDatabase = "Local Offline Cache"
        };
    }

    public async Task<LatestUpdateResponse?> GetLatestUpdateAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/updates/latest");
            if (!response.IsSuccessStatusCode ||
                response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LatestUpdateResponse>();
        }
        catch
        {
            // Update checks must never block normal app startup.
            return null;
        }
    }

    public async Task<TResponse?> GetAsync<TResponse>(
        string endpoint)
    {
        var response =
            await _httpClient.GetAsync(endpoint);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                endpoint,
                request);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                endpoint,
                request);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<TResponse>();
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response =
            await _httpClient.DeleteAsync(endpoint);

        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();

        var message = response.ReasonPhrase;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document =
                    JsonDocument.Parse(body);

                if (document.RootElement.TryGetProperty(
                        "message",
                        out var messageProperty))
                {
                    message = messageProperty.GetString();
                }
                else
                {
                    message = body;
                }
            }
            catch (JsonException)
            {
                message = body;
            }
        }

        throw new HttpRequestException(
            message ?? $"HTTP {(int)response.StatusCode}",
            inner: null,
            statusCode: response.StatusCode);
    }
}

public class HealthStatusResponse
{
    public string Status { get; set; } = "Healthy";
    public string Architecture { get; set; } = "";
    public string Mode { get; set; } = "LocalNetworkPrimary";
    public bool IsOnline { get; set; } = true;
    public bool IsNeonBackupOnline { get; set; } = false;
    public string PrimaryDatabase { get; set; } = "";
    public DateTime? LastSyncUtc { get; set; }
}

public sealed class LatestUpdateResponse
{
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}
