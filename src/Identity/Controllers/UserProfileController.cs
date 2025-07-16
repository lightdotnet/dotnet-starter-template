using Light.Exceptions;
using Light.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Monolith.Identity.Controllers;

/// <summary>
/// move to other controller for current_user can get data when login without permission
/// </summary>
public class UserProfileController(
    ICurrentUser currentUser,
    JwtTokenMananger jwtTokenMananger) : ApiControllerBase
{
    private readonly string _userId = currentUser.UserId ?? throw new UnauthorizedException();

    [HttpGet("token/list")]
    public async Task<IActionResult> GetTokens()
    {
        var res = await jwtTokenMananger.GetUserTokensAsync(_userId);
        return Ok(res);
    }

    [HttpPut("token/revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] string tokenId)
    {
        await jwtTokenMananger.RevokedAsync(_userId, tokenId);
        return Ok();
    }
}
