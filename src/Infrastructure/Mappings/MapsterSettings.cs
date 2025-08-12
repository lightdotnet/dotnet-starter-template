using Mapster;

namespace Monolith.Mappings;

public class MapsterSettings
{
    public static void Configure()
    {
        // here we will define the type conversion / Custom-mapping
        // More details at https://github.com/MapsterMapper/Mapster/wiki/Custom-mapping

        // This one is actually not necessary as it's mapped by convention

        StatusConfigure();
    }

    private static void StatusConfigure()
    {
        TypeAdapterConfig<Status, Status.ActiveStatus>
            .NewConfig()
            .Map(dest => dest, src => src.Value);
    }
}