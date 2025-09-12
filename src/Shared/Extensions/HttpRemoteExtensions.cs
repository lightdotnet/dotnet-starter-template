namespace Monolith.Extensions;

public static class HttpRemoteExtensions
{
    public static string? GetMacAddress(string? ipAddress)
    {
        var p = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "arp",
                Arguments = "-a " + ipAddress,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        var regex = new System.Text.RegularExpressions.Regex(@"([0-9A-Fa-f]{2}[-:]){5}([0-9A-Fa-f]{2})");
        var match = regex.Match(output);
        return match.Success ? match.Value : null;
    }
}
