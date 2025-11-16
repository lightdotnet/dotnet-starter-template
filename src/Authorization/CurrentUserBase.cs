using Monolith.Extensions;

namespace Monolith;

public abstract class CurrentUserBase : ICurrentUser
{
    protected virtual System.Security.Claims.ClaimsPrincipal? User { get; set; }

    public string? UserId => User?.GetUserId();

    public string? Username => User?.GetUserName();

    public string? FirstName => User?.GetFirstName();

    public string? LastName => User?.GetLastName();

    public string? FullName => $"{FirstName} {LastName}";

    public string? PhoneNumber => User?.GetPhoneNumber();

    public string? Email => User?.GetEmail();

    public bool IsAuthenticated => User?.IsAuthenticated() is true;

    public bool IsSuperUser => AppSecret.IsSuper(Username);

    public bool IsInRole(string role) => User?.IsInRole(role) is true;

    public bool HasPermission(string permission) =>
        User?.HasPermission(permission) is true || IsSuperUser;
}