using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PluginToolbox;

internal static class DotnetCmd
{
    private const string DesiredRuntimeVersion = "8.0.0";

    public static void CompileProject(string projectPath, Configuration configuration, Runtime runtime)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            ArgumentList =
            {
                "build",
                projectPath,
                "-c",
                configuration.ToString(),
                "-clp:ErrorsOnly;Summary",
                "-r",
                runtime.Identifier,
                "/p:Platform=AnyCPU",
                $"/p:RuntimeFrameworkVersion={DesiredRuntimeVersion}"
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = Process.Start(psi) ?? throw new Exception("Failed to start dotnet process");
        process.WaitForExit();

        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();

        Console.WriteLine(stdout);
        Console.WriteLine(stderr);
    }
}