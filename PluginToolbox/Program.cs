using System.Reflection;
using System.Runtime.InteropServices;

namespace PluginToolbox;

internal static class Program
{
    private static string projectRoot
    {
        get
        {
            // try getting the project directory from the assembly metadata
            Assembly asm = typeof(Program).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
            string? projectDir = attrs.FirstOrDefault(static a => a.Key == "ProjectDir")?.Value;

            // no metadata? just hardcode the path I guess
            if (string.IsNullOrWhiteSpace(projectDir))
            {
                // PluginToolbox/bin/Debug/net.6.0 = 4 directories deep
                return Path.Combine("..", "..", "..", "..");
            }
            return Path.Combine(projectDir, "..");
        }
    }

    public static bool IsWindows()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsMacOS()
        => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static bool IsLinux()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    internal static void Main(string[] args)
    {
        foreach (string arg in args)
        {
            switch (arg)
            {
                case "--build":
                    Build();
                    break;
                case "--initial-setup":
                    InitialSetup();
                    break;
            }
        }
    }

    private static void InitialSetup()
    {

    }

    private static void Build()
    {
        string prefix = (IsWindows(), IsMacOS(), IsLinux()) switch
        {
            (true, _, _) => "Windows",
            (_, true, _) => "Mac",
            (_, _, true) => "Linux",
            _            => throw new PlatformNotSupportedException()
        };

        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build");
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, recursive: true);
        }

        Directory.CreateDirectory(buildPath);

        string modDir = Path.Combine(buildPath, "ExampleMod");
        ContentPackageBuilder builder = new(
            ModName: "ExampleMod",
            ModVersion: new Version(major: 1, minor: 0, build: 0),
            GameVersion: new Version(major: 1, minor: 2, build: 8, revision: 0),
            OutPath: modDir);
        builder.Prepare();

        string clientPath = Path.Combine(modDir, "Client");
        Directory.CreateDirectory(clientPath);

        DotnetCmd.CompileProject(
            projectPath: Path.Combine(projectRoot, "ClientSource", $"{prefix}Client.csproj"),
            configuration: Configuration.Debug,
            runtime: Runtime.Windows,
            outPath: clientPath);

        builder.AddAssembly(Path.Combine(clientPath, $"{prefix}Client.dll"));

        string serverPath = Path.Combine(modDir, "Server");
        Directory.CreateDirectory(serverPath);
        DotnetCmd.CompileProject(
            projectPath: Path.Combine(projectRoot, "ServerSource", $"{prefix}Server.csproj"),
            configuration: Configuration.Debug,
            runtime: Runtime.Windows,
            outPath: serverPath);

        builder.AddAssembly(Path.Combine(serverPath, $"{prefix}Server.dll"));
        builder.Build();
    }

    private static string? AskForInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }
}