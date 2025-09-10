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

    private static string Convert(string input, string separate = "_")
    {
        string newValue = "";

        for (int i = 0; i < input.Length; i++)
            if (char.IsUpper(input[i]))
                newValue += i == 0 // first char
                    ? char.ToLower(input[i])
                    : separate + char.ToLower(input[i]); // add prefix to upper chars
            else
                newValue += input[i];

        return newValue;
    }

    public static void ModuleInjected(string moduleName)
    {
        Write("Module {name} injected", Convert(moduleName));
    }

    public static void EndpointsInjected(string moduleName)
    {
        Write("Endpoints {name} injected", Convert(moduleName));
    }
}
