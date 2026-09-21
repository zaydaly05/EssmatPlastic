using EsmatPlastic.API.DTOs.Users;
using EsmatPlastic.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "Users.View")]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Users.View")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = "Users.Create")]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request);

            if (user is null)
            {
                return BadRequest(new
                {
                    message = "Unable to create user."
                });
            }

            return Ok(user);
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
    [Authorize(Policy = "Users.Edit")]
    public async Task<ActionResult<UserResponse>> Update(
        int id,
        [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateAsync(
                id,
                request);

            if (user is null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(user);
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
    [Authorize(Policy = "Users.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _userService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "User not found."
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

    [HttpPut("{id:int}/password")]
    [Authorize(Policy = "Users.Edit")]
    public async Task<IActionResult> ChangePassword(
        int id,
        [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var changed = await _userService.ChangePasswordAsync(
                id,
                request.NewPassword);

            if (!changed)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
