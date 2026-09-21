using EsmatPlastic.API.DTOs.Permissions;
using EsmatPlastic.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(
        IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    [Authorize(Policy = "Permissions.Manage")]
    public async Task<ActionResult<List<PermissionResponse>>> GetAll()
    {
        var permissions =
            await _permissionService.GetAllAsync();

        return Ok(permissions);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Permissions.Manage")]
    public async Task<ActionResult<PermissionResponse>> GetById(
        int id)
    {
        var permission =
            await _permissionService.GetByIdAsync(id);

        if (permission is null)
        {
            return NotFound(new
            {
                message = "Permission not found."
            });
        }

        return Ok(permission);
    }
}
