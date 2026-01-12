using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace PluginToolbox;

internal static class Program
{
    private static string projectRoot
    {
        get
        {
            // try getting the project directory from the assembly metadata
            Assembly asm = typeof(Program).Assembly;
            string? projectDir = asm
                                 .GetCustomAttributes<AssemblyMetadataAttribute>()
                                 .FirstOrDefault(static a => a.Key == "SolutionRoot")
                                 ?.Value;

            // no metadata? just hardcode the path I guess
            if (string.IsNullOrWhiteSpace(projectDir))
            {
                return Path.Combine("..", "..", "..", "..");
            }

            return projectDir;
        }
    }

    public static bool IsWindows()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS()
        => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    private static string GetPlatformString()
        => (IsWindows(), IsMacOS(), IsLinux()) switch
        {
            (true, _, _) => "Windows",
            (_, true, _) => "Mac",
            (_, _, true) => "Linux",
            _ => throw new PlatformNotSupportedException()
        };

    internal static void Main(string[] args)
    {
        foreach (string arg in args)
        {
            switch (arg)
            {
                case "--build":
                    Build();
                    break;
            }
        }

        string? action = AskForInput("What to do? [build]");
        if (action == null) { return; }

        switch (action)
        {
            case "build":
                Build();
                break;
        }
    }

    private static void Build()
    {
        string prefix = GetPlatformString();

        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build");
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, recursive: true);
        }

        List<(string ProjectPath, Runtime Runtime)> projects = [
            ($@"{projectRoot}\ClientProject\WindowsClient.csproj", Runtime.Windows),
            ($@"{projectRoot}\ClientProject\LinuxClient.csproj", Runtime.Linux),
            ($@"{projectRoot}\ClientProject\MacClient.csproj", Runtime.Mac),
            ($@"{projectRoot}\ServerProject\WindowsServer.csproj", Runtime.Windows),
            ($@"{projectRoot}\ServerProject\LinuxServer.csproj", Runtime.Linux),
            ($@"{projectRoot}\ServerProject\MacServer.csproj", Runtime.Mac)
        ];

        foreach (var project in projects)
        {
            Console.WriteLine($"Building {project.ProjectPath}");
            DotnetCmd.CompileProject(project.ProjectPath, Configuration.Release, project.Runtime);
        }

        Console.WriteLine("Finished building!");
    }

    private static string? AskForInput(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine();
    }
}