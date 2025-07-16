using Serilog;
using Serilog.Core;

namespace Monolith;

public class StaticLogger
{
    public static void EnsureInitialized()
    {
        if (Log.Logger is not Logger)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(c => c.Console())
                .WriteTo.Async(c => c.File(@"logs\application-startup.txt",
                    shared: true,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 52428800)) // 50mb
                .CreateLogger();

            Log.Warning("Serilogger initialized");
        }
    }

    public static void ModuleInjected(string moduleName)
    {
        Log.Logger.Warning("Module {name} injected", Convert(moduleName));
    }

    public static void EndpointsInjected(string moduleName)
    {
        Log.Logger.Warning("Endpoints {name} injected", Convert(moduleName));
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


}
