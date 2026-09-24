using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EsmatPlastic.Shared.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _token;
    private string _baseUrl;

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
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

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
        if (!response.IsSuccessStatusCode) return default;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", content);
        if (!response.IsSuccessStatusCode) return default;

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
        return response.IsSuccessStatusCode;
    }
}
