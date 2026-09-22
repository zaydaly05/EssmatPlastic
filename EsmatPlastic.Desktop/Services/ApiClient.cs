using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EsmatPlastic.Desktop.Services;

public class ApiClient
{
    private HttpClient _httpClient;

    public ApiClient()
    {
        _httpClient = CreateHttpClient();
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            EnableMultipleHttp2Connections = true
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = new Uri("http://localhost:5023/"),
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
        if (!url.EndsWith("/")) url += "/";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            if (_httpClient.BaseAddress == uri)
                return;

            var previousClient = _httpClient;
            var replacementClient = CreateHttpClient();
            replacementClient.BaseAddress = uri;
            replacementClient.DefaultRequestHeaders.Authorization =
                previousClient.DefaultRequestHeaders.Authorization;
            _httpClient = replacementClient;
            previousClient.Dispose();
        }
    }

    public async Task<bool> TestConnectionAsync(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!url.EndsWith("/")) url += "/";

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await client.GetAsync(new Uri(new Uri(url), "api/Health/status"));
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }
        catch
        {
            return false;
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
            message ?? $"HTTP {(int)response.StatusCode}");
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
