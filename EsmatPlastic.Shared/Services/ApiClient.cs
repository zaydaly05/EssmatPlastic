using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EsmatPlastic.Shared.Configuration;
using EsmatPlastic.Shared.Models.System;

namespace EsmatPlastic.Shared.Services;

public class ApiClient
{
    private HttpClient _httpClient;
    private string? _token;

    public ApiClient()
    {
        _httpClient = CreateHttpClient(ApiConfig.GetBaseUrl());
    }

    private static HttpClient CreateHttpClient(string baseUrl)
    {
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
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
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
            return null;
        }
    }

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        var message = response.ReasonPhrase;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                if (document.RootElement.TryGetProperty("message", out var messageProperty))
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
