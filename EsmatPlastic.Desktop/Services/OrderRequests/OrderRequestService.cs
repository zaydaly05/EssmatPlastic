using EsmatPlastic.Desktop.Models.OrderRequests;

namespace EsmatPlastic.Desktop.Services.OrderRequests;

public class OrderRequestService
{
    private readonly ApiClient _apiClient;

    public OrderRequestService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<OrderRequestResponseDto>> GetAllAsync()
    {
        var result = await _apiClient.GetAsync<List<OrderRequestResponseDto>>("api/OrderRequests");
        return result ?? new List<OrderRequestResponseDto>();
    }

    public async Task CreateAsync(CreateOrderRequestDto request)
    {
        await _apiClient.PostAsync<CreateOrderRequestDto, object>("api/OrderRequests", request);
    }

    public async Task CancelAsync(int id)
    {
        await _apiClient.DeleteAsync($"api/OrderRequests/{id}");
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        // Enum int values matching API OrderRequestStatus enum:
        // Pending = 0, Approved = 1, Processing = 2, Completed = 3, Cancelled = 4
        int statusValue = status switch
        {
            "Pending" => 0,
            "Approved" => 1,
            "Processing" => 2,
            "Completed" => 3,
            "Cancelled" => 4,
            _ => 0
        };

        await _apiClient.PatchAsync($"api/OrderRequests/{id}/status", statusValue);
    }
}
