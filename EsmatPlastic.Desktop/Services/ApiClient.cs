using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EsmatPlastic.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5023/")
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
