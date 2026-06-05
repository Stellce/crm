using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Security;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[Authorize(Policy = AppPolicies.ManageUsers)]
[EnableRateLimiting(RateLimitPolicies.UserApi)]
[Route("api/users")]
[ApiController]
public class UsersController(
    UserService userService
) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst("sub")?.Value;

        if (!int.TryParse(currentUserId, out var currentUserIdInt))
            throw new AppException(ErrorCode.InvalidAccessToken);

        return Ok(await userService.GetUsers(currentUserIdInt, cancellationToken));
    }

    [HttpGet("{targetUserId:int}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int targetUserId, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst("sub")?.Value;

        if (!int.TryParse(currentUserId, out var currentUserIdInt))
            throw new AppException(ErrorCode.InvalidAccessToken);

        return Ok(await userService.GetUserById(targetUserId, currentUserIdInt, cancellationToken));
    }

    [HttpPost("manager")]
    public async Task<ActionResult<UserResponse>> CreateManager(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userService.CreateManager(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { targetUserId = user.Id }, user);
    }

    [Authorize(Policy = AppPolicies.CreateAdmins)]
    [HttpPost("admin")]
    public async Task<ActionResult<UserResponse>> CreateAdmin(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userService.CreateAdmin(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { targetUserId = user.Id }, user);
    }

    [HttpDelete("{targetUserId:int}")]
    public async Task<ActionResult> DeleteUser(int targetUserId, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst("sub")?.Value;

        if (!int.TryParse(currentUserId, out var currentUserIdInt))
            throw new AppException(ErrorCode.InvalidAccessToken);

        await userService.DeleteUser(targetUserId, currentUserIdInt, cancellationToken);
        return NoContent();
    }
}
