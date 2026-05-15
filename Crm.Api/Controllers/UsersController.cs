using Crm.Api.Dtos;
using Crm.Api.Exceptions;
using Crm.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(
        UserService userService
    ) : ControllerBase
    {
        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var currentUserId = User.FindFirst("sub")?.Value;

            if (!int.TryParse(currentUserId, out var currentUserIdInt))
                throw new AppException(ErrorCode.Unauthorized);

            return Ok(await userService.GetUsers(currentUserIdInt));
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet("{targetUserId:int}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int targetUserId)
        {
            var currentUserId = User.FindFirst("sub")?.Value;

            if (!int.TryParse(currentUserId, out var currentUserIdInt))
                throw new AppException(ErrorCode.Unauthorized);

            return Ok(await userService.GetUserById(targetUserId, currentUserIdInt));
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost("manager")]
        public async Task<ActionResult<UserResponse>> CreateManager(CreateUserRequest request)
        {
            var user = await userService.CreateManager(request);
            return CreatedAtAction(nameof(GetUserById), new { targetUserId = user.Id }, user);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("admin")]
        public async Task<ActionResult<UserResponse>> CreateAdmin(CreateUserRequest request)
        {
            var user = await userService.CreateAdmin(request);
            return CreatedAtAction(nameof(GetUserById), new { targetUserId = user.Id }, user);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
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
}
