using Monolith.Identity;
using Monolith.Notifications;

namespace Monolith.HttpApi.Identity;

public class UserProfileHttpService(IHttpClientFactory httpClientFactory)
    : HttpClientBase(httpClientFactory)
{
    private const string BasePath = "user_profile";

    public Task<Result<UserDto>> GetAsync()
    {
        var url = $"{BasePath}";
        return this.TryGetAsync<UserDto>(url);
    }

    public Task<Result<IEnumerable<UserTokenDto>>> GetTokensAsync()
    {
        var url = $"{BasePath}/token/list";
        return this.TryGetAsync<IEnumerable<UserTokenDto>>(url);
    }

    public Task<Result> RevokeTokenAsync(string tokenId)
    {
        var url = $"{BasePath}/token/revoke";
        return this.TryPutAsync(url, tokenId);
    }

    public Task<PagedResult<NotificationDto>> GetNotificationsAsync(NotificationLookup request)
    {
        var url = $"{BasePath}/notification";
        url += "?" + UriQueryBuilder.ToQueryString(request);
        return this.TryPagedAsync<NotificationDto>(url);
    }

    public Task<Result<NotificationDto>> GetNotificationByIdAsync(string id)
    {
        var url = $"{BasePath}/notification/{id}";
        return this.TryGetAsync<NotificationDto>(url);
    }

    public Task<Result<int>> CountUnreadNotificationsAsync()
    {
        var url = $"{BasePath}/notification/count_unread";
        return this.TryGetAsync<int>(url);
    }

    public Task<Result> ReadNotificationAsync(string id)
    {
        var url = $"{BasePath}/notification/read";
        return this.TryPutAsync(url, id);
    }
}