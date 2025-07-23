using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolith.Identity.Jwt;

namespace Monolith.Features.Identity;

[AllowAnonymous]
[Route("api/v{version:apiVersion}/oauth")]
public class TokenController(ITokenService tokenService) : ApiControllerBase
{
    [HttpPost("token/get")]
    public async Task<IActionResult> GetToken(
        [FromQuery] string? deviceId,
        [FromQuery] string? deviceName,
        [FromBody] GetTokenRequest request)
    {
        var res = await tokenService.GetTokenAsync(
            request.Username,
            request.Password,
            deviceId,
            deviceName);

        return Ok(res);
    }

    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var res = await tokenService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken);

        return Ok(res);
    }
}