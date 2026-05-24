using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AuthService authService
) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request)
    {
        return Ok(await authService.LoginUser(request));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshTokenRequest request)
    {
        return Ok(await authService.RefreshToken(request.RefreshToken));
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(
        [FromBody] RefreshTokenRequest request)
    {
        await authService.LogoutUser(request.RefreshToken);
        return NoContent();
    }
}