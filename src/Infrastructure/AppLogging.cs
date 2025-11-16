using Serilog;

namespace Monolith;

public class AppLogging
{
    private static Serilog.Core.Logger? _logger;

    public static Serilog.Core.Logger Logger
    {
        get
        {
            if (_logger is null)
            {
                _logger ??= new LoggerConfiguration()
                    .WriteTo.Async(c => c.Console())
                    .WriteTo.Async(c => c.File(@"logs\application-startup.txt",
                        shared: true,
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: 52428800)) // 50mb
                    .CreateLogger();

                _logger.Warning("Static Logger initialized");
            }

            return _logger;
        }
    }

    public static void Write(string message, params object[] values)
    {
        Logger.Warning(message, values);
    }
}
