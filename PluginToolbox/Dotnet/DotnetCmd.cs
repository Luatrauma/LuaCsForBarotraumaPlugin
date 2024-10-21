
using System.Diagnostics;

namespace PluginToolbox;

internal static class DotnetCmd
{
    private const string DotnetAppName = "dotnet";
    private const string DesiredRuntimeVersion = "6.0.8";

    public static void CompileProject(string projectPath, Configuration configuration, Runtime runtime, string outPath)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = DotnetAppName,
            ArgumentList =
            {
                "publish",
                projectPath,
                "-c",
                configuration.ToString(),
                "-clp:ErrorsOnly;Summary",
                "-r",
                runtime.Identifier,
                "/p:Platform=AnyCPU",
                $"/p:RuntimeFrameworkVersion={DesiredRuntimeVersion}",
                "-o",
                outPath
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