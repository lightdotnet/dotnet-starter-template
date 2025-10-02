using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolith.Extensions;
using Monolith.Identity;
using Monolith.Identity.Jwt;

namespace Monolith.Features.Identity;

[Route("api/v{version:apiVersion}/oauth")]
public class TokenController(ITokenService tokenService) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("token/get")]
    public async Task<IActionResult> GetToken(
        [FromQuery] string? deviceId,
        [FromQuery] string? deviceName,
        [FromBody] GetTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var macAddress = HttpRemoteExtensions.GetMacAddress(ipAddress);

        var res = await tokenService.GetTokenAsync(
            request.Username,
            request.Password,
            new DeviceDto
            {
                Id = deviceId,
                Name = deviceName,
                IpAddress = ipAddress,
                PhysicalAddress = macAddress,
            });

        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var macAddress = HttpRemoteExtensions.GetMacAddress(ipAddress);

        var res = await tokenService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken,
            new DeviceDto
            {
                IpAddress = ipAddress,
                PhysicalAddress = macAddress,
            });

        return Ok(res);
    }

    [HttpGet("token/check")]
    public IActionResult CheckToken()
    {
        return Ok();
    }
}