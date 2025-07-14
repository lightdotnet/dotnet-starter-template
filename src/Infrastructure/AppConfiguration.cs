using Microsoft.Extensions.Configuration;

namespace Monolith;

public static class AppConfiguration
{
    public static bool IsUseInMemoryDatabase { get; private set; } = false;

    public static void CheckUseInMemoryDatabase(this IConfiguration configuration)
    {
        IsUseInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase");
    }
}
