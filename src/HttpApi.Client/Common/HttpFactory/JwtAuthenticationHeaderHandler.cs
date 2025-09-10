using Monolith.HttpApi.Common.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace Monolith.HttpApi.Common.HttpFactory;

public class JwtAuthenticationHeaderHandler(ITokenProvider tokenProvider)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath;

        // skip token endpoints
        if (path?.Contains("oauth/token") is not true)
        {
            var token = await tokenProvider.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(Result.Unauthorized()),
                        System.Text.Encoding.UTF8,
                        MediaTypeNames.Application.Json)
                };
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}