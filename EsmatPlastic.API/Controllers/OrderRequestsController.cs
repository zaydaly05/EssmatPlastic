using Microsoft.AspNetCore.Mvc;
using EsmatPlastic.API.DTOs.OrderRequests;
using EsmatPlastic.API.Services;
using EsmatPlastic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using EsmatPlastic.Domain.Enums;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderRequestsController : ControllerBase
{
    private readonly IOrderRequestService _orderRequestService;

    public OrderRequestsController(IOrderRequestService orderRequestService)
    {
        _orderRequestService = orderRequestService;
    }

    [HttpPost]
    [Authorize(Roles = "Secretary,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var items = request.Items.Select(i => (i.ProductVariantId, i.Quantity)).ToList();
        var id = await _orderRequestService.CreateRequestAsync(userId, request.CustomerName, request.CustomerPhone, items);

        return Ok(new { Id = id });
    }

    [HttpGet]
    [Authorize(Roles = "Secretary,Admin")]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _orderRequestService.GetAllRequestsAsync();
        var response = requests.Select(r => new OrderRequestResponseDto
        {
            Id = r.Id,
            CustomerName = r.CustomerName,
            CustomerPhone = r.CustomerPhone,
            RequestedAt = r.RequestedAt,
            Status = r.Status.ToString(),
            Items = r.Items.Select(i => new OrderItemResponseDto
            {
                ProductVariantId = i.ProductVariantId,
                VariantName = i.ProductVariant.Name,
                Quantity = i.Quantity
            }).ToList()
        }).ToList();

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Secretary,Admin")]
    public async Task<IActionResult> Cancel(int id)
    {
        var success = await _orderRequestService.CancelRequestAsync(id);
        return success ? Ok() : BadRequest("Cannot cancel request.");
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderRequestStatus status)
    {
        var success = await _orderRequestService.UpdateStatusAsync(id, status);
        return success ? Ok() : BadRequest("Could not update status.");
    }
}
