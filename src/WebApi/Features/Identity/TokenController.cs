using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var res = await tokenService.GetTokenAsync(
            request.Username,
            request.Password,
            deviceId,
            deviceName);

        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var res = await tokenService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken);

        return Ok(res);
    }

    [HttpGet("token/check")]
    public IActionResult CheckToken()
    {
        return Ok();
    }
}