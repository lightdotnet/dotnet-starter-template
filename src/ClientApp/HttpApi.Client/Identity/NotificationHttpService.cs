using Monolith.Notifications;

namespace Monolith.Identity;

public class NotificationHttpService(IHttpClientFactory httpClientFactory) :
    TryHttpClient(httpClientFactory)
{
    private const string _path = "api/v1/notification";

    public Task<PagedResult<NotificationDto>> GetAsync(NotificationLookup request)
    {
        var url = $"{_path}";
        url += "?" + UriQueryBuilder.ToQueryString(request);

        return TrySearchAsync<NotificationDto>(url);
    }

    public Task<Result<NotificationDto>> GetByIdAsync(string id)
    {
        var url = $"{_path}/{id}";

        return TryGetAsync<NotificationDto>(url);
    }

    public Task<Result<int>> CountUnreadAsync(string userId)
    {
        var url = $"{_path}/{userId}/unread/count";

        return TryGetAsync<int>(url);
    }

    public Task<Result<NotificationDto>> ReadAsync(string id)
    {
        var url = $"{_path}/read/{id}";

        return TryGetAsync<NotificationDto>(url);
    }

    public Task<Result> CreateAsync(string fromUserId, string? fromName, string toUserId, SystemMessage request)
    {
        var url = $"{_path}";

        url += "?" + UriQueryBuilder.ToQueryString(new
        {
            fromUserId,
            fromName,
            toUserId
        });

        return TryPostAsync(url, request);
    }
}
