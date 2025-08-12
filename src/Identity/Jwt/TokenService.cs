﻿using Light.ActiveDirectory.Interfaces;
using Light.Identity.EntityFrameworkCore;
using Light.Identity.Extensions;
using Light.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ClaimTypes = Light.Identity.ClaimTypes;

namespace Monolith.Identity.Jwt;

internal class TokenService(
    IOptions<JwtOptions> jwtOptions,
    JwtTokenMananger jwtTokenMananger,
    IActiveDirectoryService domainService) : ITokenService
{
    private readonly UserManager<User> _userManager = jwtTokenMananger.UserManager;

    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<IResult<TokenDto>> GetTokenAsync(
        string username, string password,
        string? deviceId = null, string? deviceName = null)
    {
        var user = await _userManager.FindByNameAsync(username);

        var errorResult = Result<TokenDto>.Error("Invalid credentials");

        if (user is null || await CheckInvalidUser(user))
            return errorResult;

        bool isPasswordValid;

        if (user.AuthProvider == AuthProvider.AD.ToString())
        {
            isPasswordValid = await domainService.CheckPasswordSignInAsync(username, password);
        }
        else
        {
            var checkLocalPassword = await _userManager.CheckPasswordAsync(user, password);
            isPasswordValid = checkLocalPassword;
        }

        if (isPasswordValid is false)
        {
            return errorResult;
        }

        var tokenExpiresAt = DateTime.Now.AddSeconds(_jwt.AccessTokenExpirationSeconds);
        var refreshTokenExpiresAt = DateTime.Today.AddDays(_jwt.RefreshTokenExpirationDays);

        return await jwtTokenMananger.GenerateTokenByAsync(
            user,
            _jwt.Issuer,
            _jwt.SecretKey,
            tokenExpiresAt,
            refreshTokenExpiresAt,
            deviceId,
            deviceName);
    }

    public async Task<IResult<TokenDto>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        // get UserPrincipal from expired token
        var userPrincipal = JwtHelper.GetPrincipalFromExpiredToken(
            accessToken,
            jwtOptions.Value.Issuer,
            jwtOptions.Value.SecretKey,
            ClaimTypes.Role);

        // get userID from UserPrincipal
        var userId = userPrincipal.FindFirstValue(ClaimTypes.UserId);

        if (string.IsNullOrEmpty(userId))
            return Result<TokenDto>.Unauthorized("Error when read info from token.");

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || await CheckInvalidUser(user))
            return Result<TokenDto>.Unauthorized("Invalid credentials.");

        var tokenExpiresAt = DateTime.Now.AddSeconds(_jwt.AccessTokenExpirationSeconds);
        var refreshTokenExpiresAt = DateTime.Today.AddDays(_jwt.RefreshTokenExpirationDays);

        return await jwtTokenMananger.RefreshTokenAsync(
            user,
            refreshToken,
            _jwt.Issuer,
            _jwt.SecretKey,
            tokenExpiresAt,
            refreshTokenExpiresAt,
            ClaimTypes.Role, ClaimTypes.UserId);
    }

    public virtual Task<bool> CheckInvalidUser(User user)
    {
        var isInvalid = user.Status.IsActive is false // use is not active
            || user.Deleted != null; // user is deleted

        return Task.FromResult(isInvalid);
    }
}
