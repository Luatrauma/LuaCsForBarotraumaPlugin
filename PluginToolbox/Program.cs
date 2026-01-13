namespace PluginToolbox;

internal static class Program
{
    private static string ProjectRoot
    {
        get
        {
            return Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, "..", "..", "..", "..");
        }
    }

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
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build");
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, recursive: true);
        }

        List<(string ProjectPath, Runtime Runtime)> projects = [
            ($@"{ProjectRoot}\ClientProject\WindowsClient.csproj", Runtime.Windows),
            ($@"{ProjectRoot}\ClientProject\LinuxClient.csproj", Runtime.Linux),
            ($@"{ProjectRoot}\ClientProject\MacClient.csproj", Runtime.Mac),
            ($@"{ProjectRoot}\ServerProject\WindowsServer.csproj", Runtime.Windows),
            ($@"{ProjectRoot}\ServerProject\LinuxServer.csproj", Runtime.Linux),
            ($@"{ProjectRoot}\ServerProject\MacServer.csproj", Runtime.Mac)
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