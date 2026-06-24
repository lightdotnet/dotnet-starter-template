using Monolith.Notifications;

namespace Monolith.HttpApi.Identity;

public class NotificationHttpService(IHttpClientFactory httpClientFactory)
    : HttpClientBase(httpClientFactory)
{
    protected override string ClientName => HttpClientConstants.IdentityApi;

    private const string _path = "notification";

    public Task<PagedResult<NotificationDto>> GetAsync(NotificationLookup request)
    {
        var url = $"{_path}";
        url += "?" + UriQueryBuilder.ToQueryString(request);

        return this.TryPagedAsync<NotificationDto>(url);
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

        return this.TryPostAsync(url, request);
    }
}
