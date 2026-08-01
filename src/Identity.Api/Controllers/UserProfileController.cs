using Light.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using StarterKit.Identity.Api.Jwt;
using StarterKit.Identity.Contracts.Services;
using StarterKit.Infrastructure.Endpoints;
using StarterKit.Shared;

namespace StarterKit.Identity.Api.Controllers;

[ApiExplorerSettings(GroupName = "identity")]
[Route("api/v{version:apiVersion}/user_profile")]
public class UserProfileController(
    ICurrentUser currentUser,
    IUserService userService,
    IUserSessionService userSessionService) : VersionedApiController
{
    private readonly string _userId = currentUser.UserId ?? throw new UnauthorizedException();

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
        var isTokenValid = await userSessionService.IsTokenValidAsync(accessToken);

        if (isTokenValid is false)
        {
            return Ok(Result.Unauthorized());
        }

        var res = await userService.GetByIdAsync(_userId);

        if (res.Data?.Status != ActiveStatus.State.Active.ToString())
        {
            return Ok(Result.Unauthorized());
        }

        return Ok(res);
    }

    [HttpGet("token/list")]
    public async Task<IActionResult> GetTokens()
    {
        var res = await userSessionService.GetUserTokensAsync(_userId);
        return Ok(res);
    }

    [HttpPut("token/revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] string tokenId)
    {
        await userSessionService.RevokeAsync(_userId, tokenId);
        return Ok();
    }
}
