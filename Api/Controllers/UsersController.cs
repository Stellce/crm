using Api.Dtos;
using Api.Exceptions;
using Api.Security;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Policy = AppPolicies.ManageUsers)]
[Route("api/[controller]")]
[ApiController]
public class UsersController(
    UserService userService
) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var currentUserId = User.FindFirst("sub")?.Value;

        if (!int.TryParse(currentUserId, out var currentUserIdInt))
            throw new AppException(ErrorCode.Unauthorized);

        return Ok(await userService.GetUsers(currentUserIdInt));
    }

    [HttpGet("{targetUserId:int}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int targetUserId)
    {
        var currentUserId = User.FindFirst("sub")?.Value;

        if (!int.TryParse(currentUserId, out var currentUserIdInt))
            throw new AppException(ErrorCode.Unauthorized);

        return Ok(await userService.GetUserById(targetUserId, currentUserIdInt));
    }

    [HttpPost("manager")]
    public async Task<ActionResult<UserResponse>> CreateManager(CreateUserRequest request)
    {
        var user = await userService.CreateManager(request);
        return CreatedAtAction(nameof(GetUserById), new { targetUserId = user.Id }, user);
    }

    [Authorize(Policy = AppPolicies.CreateAdmins)]
    [HttpPost("admin")]
    public async Task<ActionResult<UserResponse>> CreateAdmin(CreateUserRequest request)
    {
        var user = await userService.CreateAdmin(request);
        return CreatedAtAction(nameof(GetUserById), new { targetUserId = user.Id }, user);
    }

    [HttpDelete("{targetUserId:int}")]
    public async Task<ActionResult> DeleteUser(int targetUserId)
    {
        var currentUserId = User.FindFirst("sub")?.Value;

        if (!int.TryParse(currentUserId, out var currentUserIdInt))
            throw new AppException(ErrorCode.Unauthorized);

        await userService.DeleteUser(targetUserId, currentUserIdInt);
        return NoContent();
    }
}
