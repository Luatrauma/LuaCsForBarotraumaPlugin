namespace PluginToolbox;

internal static class Program
{
    // PluginToolbox/bin/Debug/net.6.0 = 4 directories deep
    private static readonly string projectRoot = Path.Combine("..", "..", "..", "..");

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
            projectPath: Path.Combine(projectRoot, "ClientSource", "WindowsClient.csproj"),
            configuration: Configuration.Debug,
            runtime: Runtime.Windows,
            outPath: clientPath);

        builder.AddAssembly(Path.Combine(clientPath, "WindowsClient.dll"));

        string serverPath = Path.Combine(modDir, "Server");
        Directory.CreateDirectory(serverPath);
        DotnetCmd.CompileProject(
            projectPath: Path.Combine(projectRoot, "ServerSource", "WindowsServer.csproj"),
            configuration: Configuration.Debug,
            runtime: Runtime.Windows,
            outPath: serverPath);

        builder.AddAssembly(Path.Combine(serverPath, "WindowsServer.dll"));
        builder.Build();
    }

    private static string? AskForInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }
}