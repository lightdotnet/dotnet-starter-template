using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Identity.Api.Jwt;
using StarterKit.Identity.Contracts;
using StarterKit.Infrastructure.Endpoints;

namespace StarterKit.Identity.Api.Controllers;

[ApiExplorerSettings(GroupName = "identity")]
public class TokenController(
    IAuthenticationService authenticationService) : VersionedApiController
{
    [AllowAnonymous]
    [HttpPost("token/get")]
    public async Task<IActionResult> GetToken(
        [FromQuery] string? deviceId,
        [FromQuery] string? deviceName,
        [FromBody] GetTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var res = await authenticationService.GetTokenAsync(
            request.Username,
            request.Password,
            new DeviceDto
            {
                Id = deviceId,
                Name = deviceName,
                IpAddress = ipAddress,
            });

        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var res = await authenticationService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken,
            new DeviceDto
            {
                IpAddress = ipAddress,
            });

        return Ok(res);
    }
}