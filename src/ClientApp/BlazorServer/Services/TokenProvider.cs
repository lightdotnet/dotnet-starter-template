using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.Blazor.Services
{
    public class TokenProvider(ITokenManager tokenManager) : ITokenProvider
    {
        private string? AccessToken;
        
        public async Task<string?> GetAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                var getSavedToken = await tokenManager.GetSavedTokenAsync();

                AccessToken = getSavedToken?.Token;
            }

            return AccessToken;
        }
    }
}
