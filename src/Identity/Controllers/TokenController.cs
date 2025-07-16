using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolith.Identity.Dtos;

namespace Monolith.Identity.Controllers;

[AllowAnonymous]
[Route("api/v{version:apiVersion}/oauth")]
public class TokenController(ILoginService loginService) : ApiControllerBase
{
    [HttpPost("token/get")]
    public async Task<IActionResult> GetToken(
        [FromQuery] string? deviceId,
        [FromQuery] string? deviceName,
        [FromBody] GetTokenRequest request)
    {
        var res = await loginService.GetTokenAsync(
            request.Username,
            request.Password,
            deviceId,
            deviceName);

        return Ok(res);
    }

    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var res = await loginService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken);

        return Ok(res);
    }
}