using EsmatPlastic.API.DTOs.ProductVariants;
using EsmatPlastic.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductVariantsController : ControllerBase
{
    private readonly IProductVariantService _variantService;

    public ProductVariantsController(
        IProductVariantService variantService)
    {
        _variantService = variantService;
    }

    [HttpGet("product/{productId:int}")]
    [Authorize(Policy = "Products.View")]
    public async Task<ActionResult<List<ProductVariantResponse>>> GetByProduct(
        int productId)
    {
        var variants = await _variantService
            .GetByProductIdAsync(productId);

        return Ok(variants);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Products.View")]
    public async Task<ActionResult<ProductVariantResponse>> GetById(
        int id)
    {
        var variant = await _variantService.GetByIdAsync(id);

        if (variant is null)
        {
            return NotFound(new
            {
                message = "Product variant not found."
            });
        }

        return Ok(variant);
    }

    [HttpPost]
    [Authorize(Policy = "Products.Create")]
    public async Task<ActionResult<ProductVariantResponse>> Create(
        [FromBody] CreateProductVariantRequest request)
    {
        try
        {
            var variant = await _variantService.CreateAsync(request);

            if (variant is null)
            {
                return BadRequest(new
                {
                    message = "Variant name is required."
                });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = variant.Id },
                variant);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Products.Edit")]
    public async Task<ActionResult<ProductVariantResponse>> Update(
        int id,
        [FromBody] UpdateProductVariantRequest request)
    {
        try
        {
            var variant = await _variantService.UpdateAsync(
                id,
                request);

            if (variant is null)
            {
                return NotFound(new
                {
                    message = "Product variant not found."
                });
            }

            return Ok(variant);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Products.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _variantService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Product variant not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}
