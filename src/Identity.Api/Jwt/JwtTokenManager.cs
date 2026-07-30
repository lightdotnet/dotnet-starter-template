using Light.Exceptions;
using Microsoft.AspNetCore.Identity;
using StarterKit.Identity.Api.Data;
using StarterKit.Identity.Api.Entities;
using StarterKit.Identity.Contracts;
using StarterKit.Shared.Constants;
using System.Security.Claims;

namespace StarterKit.Identity.Api.Jwt;

public class JwtTokenManager(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IdentityDbContext context)
{
    public UserManager<User> UserManager => userManager;

    public virtual DateTimeOffset TimeNow => DateTimeOffset.Now;

    public virtual async Task<IList<Claim>> GetUserClaimsAsync(User user)
    {
        //var userClaims = await userManager.GetClaimsAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        //var roleClaims = new List<Claim>();
        var permissionClaims = new List<Claim>();

        foreach (var userRole in userRoles)
        {
            //roleClaims.Add(new Claim(ClaimTypes.Role, userRole));

            var role = await roleManager.FindByNameAsync(userRole);
            if (role is null)
                continue;

            var roleClaims = await roleManager.GetClaimsAsync(role);

            permissionClaims.AddRange(roleClaims);
        }

        var claims = new List<Claim>
        {
            { ClaimTypeConstants.UserId, user.Id },
            { ClaimTypeConstants.UserName, user.UserName },
            //{ ClaimTypes.FirstName, user.FirstName },
            //{ ClaimTypes.LastName, user.LastName },
            //{ ClaimTypes.PhoneNumber, user.PhoneNumber },
            //{ ClaimTypes.Email, user.Email },
        }
        //.Union(userClaims)
        //.Union(roleClaims)
        .Union(permissionClaims)
        .Where(x => !string.IsNullOrEmpty(x.Value))
        .ToList();

        return claims;
    }

    public virtual async Task<TokenDto> GenerateTokenByAsync(
        User user,
        string issuer, string secretKey,
        DateTime tokenExpiresAt, DateTime refreshTokenExpiresAt,
        DeviceDto? device = null,
        bool saveToken = true)
    {
        var newToken = new UserSession
        {
            UserId = user.Id,
            TokenExpiresAt = tokenExpiresAt,
            RefreshToken = JwtHelper.GenerateRefreshToken(),
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            DeviceId = device?.Id,
            DeviceName = device?.Name,
            IpAddress = device?.IpAddress,
            PhysicalAddress = device?.PhysicalAddress,
        };

        var claims = await GetUserClaimsAsync(user);

        // add TokenID
        claims.Add(new Claim(ClaimTypeConstants.TokenId, newToken.Id));

        var jwtToken = JwtHelper.GenerateToken(
            issuer,
            claims,
            tokenExpiresAt,
            secretKey);

        if (saveToken is true)
        {
            newToken.Token = jwtToken;
        }

        await context.UserSessions.AddAsync(newToken);
        await context.SaveChangesAsync();

        // *** note: must return jwtToken cause save token to DB is options
        return new TokenDto(jwtToken, newToken.TokenExpiresInSeconds, newToken.RefreshToken);
    }

    public virtual async Task<TokenDto> RefreshTokenAsync(
        User user,
        string refreshToken,
        string issuer, string secretKey,
        DateTime tokenExpiresAt, DateTime refreshTokenExpiresAt,
        string roleClaimType = ClaimTypeConstants.Role, string userIdClaimType = ClaimTypeConstants.UserId,
        DeviceDto? device = null,
        bool saveToken = true)
    {
        // check refresh token is exist and not out of lifetime
        var userToken = await context.UserSessions
            .Where(x =>
                x.UserId == user.Id
                && x.RefreshToken == refreshToken
                && x.RefreshTokenExpiresAt >= TimeNow.Date
                && x.Revoked == false)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedException("Refresh token invalid.");

        var claims = await GetUserClaimsAsync(user);

        // add TokenID
        claims.Add(new Claim(ClaimTypeConstants.TokenId, userToken.Id));

        var timeNow = TimeNow.DateTime;

        var jwtToken = JwtHelper.GenerateToken(
            issuer,
            claims,
            tokenExpiresAt,
            secretKey);

        if (saveToken is true)
        {
            userToken.Token = jwtToken;
        }

        userToken.TokenExpiresAt = tokenExpiresAt;
        userToken.RefreshToken = JwtHelper.GenerateRefreshToken();
        userToken.RefreshTokenExpiresAt = refreshTokenExpiresAt;

        userToken.DeviceId = device?.Id;
        userToken.DeviceName = device?.Name;
        userToken.IpAddress = device?.IpAddress;
        userToken.PhysicalAddress = device?.PhysicalAddress;

        await context.SaveChangesAsync();

        // *** note: must return jwtToken cause save token to DB is options
        return new TokenDto(jwtToken, userToken.TokenExpiresInSeconds, userToken.RefreshToken);
    }

    public async Task<IEnumerable<UserSessionDto>> GetUserTokensAsync(string userId)
    {
        var now = TimeNow;

        var list = await context.UserSessions
            .Where(x =>
                x.UserId == userId
                &&
                    (x.TokenExpiresAt >= now
                    || (x.RefreshTokenExpiresAt.HasValue && x.RefreshTokenExpiresAt >= now))
                && x.Revoked == false)
            .AsNoTracking()
            .Select(s => new UserSessionDto
            {
                Id = s.Id,
                ExpiresAt = s.TokenExpiresAt,
                RefreshTokenExpiresAt = s.RefreshTokenExpiresAt,
                Device = new DeviceDto
                {
                    Id = s.DeviceId,
                    Name = s.DeviceName,
                    IpAddress = s.IpAddress,
                    PhysicalAddress = s.PhysicalAddress,
                },
            })
            .ToListAsync();

        return list;
    }

    public Task<bool> IsTokenValidAsync(string accessToken)
    {
        return context.UserSessions
            .Where(x =>
                x.Token == accessToken
                && x.Revoked == false
                && x.TokenExpiresAt > TimeNow)
            .AnyAsync();
    }

    public Task RevokedAsync(string userId, string tokenId)
    {
        return context.UserSessions
            .Where(x => x.Id == tokenId && x.UserId == userId)
            .ExecuteUpdateAsync(e => e.SetProperty(p => p.Revoked, true));
    }
}
