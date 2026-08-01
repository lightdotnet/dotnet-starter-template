using Light.Exceptions;
using StarterKit.Identity.Api.Data;
using StarterKit.Identity.Api.Entities;
using StarterKit.Identity.Contracts;

namespace StarterKit.Identity.Api.Jwt;

internal class UserSessionService(
    JwtTokenIssuer tokenIssuer,
    IdentityDbContext context,
    TimeProvider timeProvider) : IUserSessionService
{
    public virtual async Task<TokenDto> GenerateTokenAsync(
        User user,
        string issuer, string secretKey,
        DateTime tokenExpiresAt, DateTime refreshTokenExpiresAt,
        DeviceDto? device = null)
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

        var jwtToken = await tokenIssuer.IssueAsync(user, newToken.Id, issuer, secretKey, tokenExpiresAt);

        newToken.Token = jwtToken;

        await context.UserSessions.AddAsync(newToken);
        await context.SaveChangesAsync();

        return new TokenDto(jwtToken, newToken.TokenExpiresInSeconds, newToken.RefreshToken);
    }

    public virtual async Task<TokenDto> RefreshTokenAsync(
        User user,
        string refreshToken,
        string issuer, string secretKey,
        DateTime tokenExpiresAt, DateTime refreshTokenExpiresAt,
        DeviceDto? device = null)
    {
        var now = timeProvider.GetUtcNow();
        
        // check refresh token is exist and not out of lifetime
        var userToken = await context.UserSessions
            .Where(x =>
                x.UserId == user.Id
                && x.RefreshToken == refreshToken
                && x.RefreshTokenExpiresAt >= now.Date
                && x.Revoked == false)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedException("Refresh token invalid.");

        var jwtToken = await tokenIssuer.IssueAsync(user, userToken.Id, issuer, secretKey, tokenExpiresAt);

        userToken.Token = jwtToken;
        userToken.TokenExpiresAt = tokenExpiresAt;
        userToken.RefreshToken = JwtHelper.GenerateRefreshToken();
        userToken.RefreshTokenExpiresAt = refreshTokenExpiresAt;

        userToken.DeviceId = device?.Id;
        userToken.DeviceName = device?.Name;
        userToken.IpAddress = device?.IpAddress;
        userToken.PhysicalAddress = device?.PhysicalAddress;

        await context.SaveChangesAsync();

        return new TokenDto(jwtToken, userToken.TokenExpiresInSeconds, userToken.RefreshToken);
    }

    public async Task<IEnumerable<UserSessionDto>> GetUserTokensAsync(string userId)
    {
        var now = timeProvider.GetUtcNow();

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
        var now = timeProvider.GetUtcNow();
        
        return context.UserSessions
            .Where(x =>
                x.Token == accessToken
                && x.Revoked == false
                && x.TokenExpiresAt > now)
            .AsNoTracking()
            .AnyAsync();
    }

    public Task RevokeAsync(string userId, string tokenId)
    {
        return context.UserSessions
            .Where(x => x.Id == tokenId && x.UserId == userId)
            .ExecuteUpdateAsync(e => e.SetProperty(p => p.Revoked, true));
    }
}
