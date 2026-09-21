using System.Security.Claims;
using EsmatPlastic.API.DTOs.Stock;
using EsmatPlastic.API.Services;
using EsmatPlastic.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost("transaction")]
    public async Task<ActionResult<StockTransactionResponse>> CreateTransaction(
        [FromBody] CreateStockTransactionRequest request)
    {
        var requiredPermission = request.Type switch
        {
            StockTransactionType.In => "Stock.In",
            StockTransactionType.Out => "Stock.Out",
            _ => null
        };

        if (requiredPermission is null)
        {
            return BadRequest(new
            {
                message = "Invalid stock transaction type."
            });
        }

        if (!HasPermission(requiredPermission))
        {
            return Forbid();
        }

        var userIdClaim = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid user identity."
            });
        }

        try
        {
            var transaction =
                await _stockService.CreateTransactionAsync(
                    userId,
                    request);

            if (transaction is null)
            {
                return BadRequest(new
                {
                    message = "Unable to create stock transaction."
                });
            }

            return Ok(transaction);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("transactions")]
    [Authorize(Policy = "Stock.View")]
    public async Task<ActionResult<List<StockTransactionResponse>>>
        GetTransactions([FromQuery] int? productVariantId = null)
    {
        var transactions =
            await _stockService.GetTransactionsAsync(
                productVariantId);

        return Ok(transactions);
    }

    [HttpGet("current")]
    [Authorize(Policy = "Stock.View")]
    public async Task<ActionResult<List<StockBalanceResponse>>>
        GetCurrentStock()
    {
        var stock =
            await _stockService.GetCurrentStockAsync();

        return Ok(stock);
    }

    [HttpGet("current/{productVariantId:int}")]
    [Authorize(Policy = "Stock.View")]
    public async Task<ActionResult<StockBalanceResponse>>
        GetVariantStock(int productVariantId)
    {
        var stock =
            await _stockService.GetVariantStockAsync(
                productVariantId);

        if (stock is null)
        {
            return NotFound(new
            {
                message = "Product variant not found."
            });
        }

        return Ok(stock);
    }

    private bool HasPermission(string permission)
    {
        return User.FindAll("Permission")
            .Any(x => x.Value.Equals(
                permission,
                StringComparison.OrdinalIgnoreCase));
    }
}