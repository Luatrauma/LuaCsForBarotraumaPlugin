using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PluginToolbox;

internal static class DotnetCmd
{
    private static readonly Version DesiredRuntimeVersion = new(6, 0, 8);

    private static string? FindDotNet()
    {
        // check if dotnet is in PATH
        string globalDotNetPath = Where("dotnet");
        if (!string.IsNullOrWhiteSpace(globalDotNetPath))
        {
            // we have one, let's see if it has the desired runtime
            if (HasDesiredRuntime(DotNetListSdks(globalDotNetPath)))
            {
                // bingo!
                return globalDotNetPath;
            }

            // nope, let's keep looking, maybe there's another dotnet installation with the desired runtime
        }

        // ~/.dotnet/dotnet on Unix, %USERPROFILE%\.dotnet\dotnet.exe on Windows
        string homeDotNetPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotnet",
            Program.IsWindows() ? "dotnet.exe" : "dotnet");

        // maybe it's here?
        if (File.Exists(homeDotNetPath))
        {
            // let's see if it has the desired runtime
            if (HasDesiredRuntime(DotNetListSdks(homeDotNetPath)))
            {
                return homeDotNetPath;
            }
        }

        // yeah, no luck
        return null;

        static string[] DotNetListSdks(string path)
        {
            string output = RunCommand(path, "--list-sdks");
            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        static bool HasDesiredRuntime(string[] sdks)
        {
            foreach (string sdk in sdks)
            {
                // 6.0.8 [/path/to/sdk]
                string[] parts = sdk.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length < 2) { throw new Exception($"Unexpected output from dotnet --list-sdks: {sdk}"); }

                string version = parts[0];
                if (!Version.TryParse(version, out Version? sdkVersion)) { continue; }

                if (sdkVersion.Major == DesiredRuntimeVersion.Major &&
                    sdkVersion.Minor == DesiredRuntimeVersion.Minor)
                {
                    return true;
                }
            }

            return false;
        }

        static string Where(string command)
            => RunCommand(Program.IsWindows() ? "where.exe" : "which", command);

        static string RunCommand(string command, string arg)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = command,
                ArgumentList = { arg },
                RedirectStandardOutput = true
            };

            var process = Process.Start(psi) ?? throw new Exception("Failed to start process");
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd().Trim();
            return output;
        }
    }

    public static void CompileProject(string projectPath, Configuration configuration,
                                      Runtime runtime, string outPath)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = FindDotNet() ?? throw new Exception("dotnet not found"),
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
                $"/p:RuntimeFrameworkVersion={DesiredRuntimeVersion.Major}.{DesiredRuntimeVersion.Minor}.{DesiredRuntimeVersion.Build}",
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