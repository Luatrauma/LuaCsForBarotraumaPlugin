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
                // PluginToolbox/bin/Debug/net.6.0 = 4 directories deep
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
                case "--update-metadata":
                    UpdateMetadata();
                    break;
            }
        }
    }

    private static void InitialSetup()
    {
        string modName = AskForInput("Enter the mod name") ?? string.Empty;
        if (!IsValid(modName))
        {
            Console.WriteLine("Mod name cannot be empty");
            return;
        }

        string author = AskForInput("Enter the author(s) name") ?? string.Empty;
        if (!IsValid(author))
        {
            Console.WriteLine("Author name cannot be empty");
            return;
        }

        string identifier = AskForInput("Enter the mod identifier (leave blank for auto generated one)") ??
                            string.Empty;
        if (!IsValid(identifier))
        {
            identifier =
                $"{FormatIdentifier(author, "_", allowNumbers: true)}.{FormatIdentifier(modName, "_", allowNumbers: true)}";
            Console.WriteLine($"Generated identifier: {identifier}");
        }

        string repo = AskForInput("Enter the repository URL (leave blank for none)") ?? string.Empty;

        (XDocument Doc, string Path)[] projects = GetCsprojFilesToUpdate();

        (XDocument Doc, string Path) currentPlatformProject =
            projects.First(static p => Path.GetFileName(path: p.Path).Contains(GetPlatformString()));
        Version gameVersion = GetGameVersionFromCsproj(currentPlatformProject.Doc, currentPlatformProject.Path);

        foreach (var (csproj, path) in projects)
        {
            Csproj.SetVersion(csproj, new Version(major: 1, minor: 0, build: 0));
            Csproj.SetAssemblyName(csproj, modName);
            Csproj.SetRootNamespace(csproj, FormatIdentifier(modName, string.Empty, allowNumbers: false));
            Csproj.SetAuthors(csproj, author);
            Csproj.SetRepositoryUrl(csproj, repo);

            Csproj.SetBaroMetadata(csproj,
                                   Metadata.GameVersion(gameVersion),
                                   Metadata.Identifier(identifier),
                                   Metadata.RepositoryUrl(repo));
            Csproj.SaveCsproj(csproj, path);
        }

        static bool IsValid(string? answer)
            => !string.IsNullOrWhiteSpace(answer);

        static string FormatIdentifier(string id, string sub, bool allowNumbers)
        {
            StringBuilder sb = new();
            foreach (char c in id)
            {
                if (asciiLetters.Contains(c) || (allowNumbers && asciiDigits.Contains(c)))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(sub);
                }
            }

            return sb.ToString();
        }
    }

    private static Version GetGameVersionFromCsproj(XDocument csproj, string path)
    {
        string oldDir = Directory.GetCurrentDirectory();
        string? assemblyPath = Csproj.GetBaroAssemblyPath(csproj);

        if (assemblyPath is null)
        {
            throw new InvalidOperationException("Could not find assembly path in csproj");
        }

        Directory.SetCurrentDirectory(Path.GetDirectoryName(path)!);
        AssemblyName name = AssemblyName.GetAssemblyName(assemblyPath);
        Directory.SetCurrentDirectory(oldDir);
        return name.Version!;
    }

    private static readonly char[] asciiLetters
        = Enumerable.Range('A', 'Z').Concat(Enumerable.Range('a', 'z'))
                    .Select(static i => (char)i)
                    .ToArray();

    private static readonly char[] asciiDigits
        = Enumerable.Range('0', '9')
                    .Select(static i => (char)i)
                    .ToArray();

    public static (XDocument Doc, string Path)[] GetCsprojFilesToUpdate()
    {
        string clientPath = Path.Combine(projectRoot, "ClientSource");
        string serverPath = Path.Combine(projectRoot, "ServerSource");

        string[] clientCsprojs = Directory.GetFiles(clientPath, "*Client.csproj", SearchOption.TopDirectoryOnly);
        string[] serverCsprojs = Directory.GetFiles(serverPath, "*Server.csproj", SearchOption.TopDirectoryOnly);

        List<(XDocument, string)> docs = new();
        docs.AddRange(clientCsprojs.Select(static path => (Csproj.ParseCsproj(path), path)));
        docs.AddRange(serverCsprojs.Select(static path => (Csproj.ParseCsproj(path), path)));

        return docs.ToArray();
    }

    private static void UpdateMetadata()
    {
        (XDocument Doc, string Path)[] projects = GetCsprojFilesToUpdate();

        (XDocument Doc, string Path) currentPlatformProject =
            projects.First(static c => Path.GetFileName(path: c.Path).Contains(GetPlatformString()));

        Version gameVersion = GetGameVersionFromCsproj(currentPlatformProject.Doc, currentPlatformProject.Path);

        foreach (var (csproj, path) in projects)
        {
            Csproj.SetBaroMetadata(csproj, Metadata.GameVersion(gameVersion));
            Csproj.SaveCsproj(csproj, path);
        }
    }

    private static string GetPlatformString()
        => (IsWindows(), IsMacOS(), IsLinux()) switch
        {
            (true, _, _) => "Windows",
            (_, true, _) => "Mac",
            (_, _, true) => "Linux",
            _            => throw new PlatformNotSupportedException()
        };

    private static void Build()
    {
        string prefix = GetPlatformString();

        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build");
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, recursive: true);
        }

        Directory.CreateDirectory(buildPath);

        string modDir = Path.Combine(buildPath, "ExampleMod");
        ContentPackageBuilder builder = new(
            ModName: "ExampleMod",                                 // PLUGIN_TODO get mod name automatically
            ModVersion: new Version(major: 1, minor: 0, build: 0), // PLUGIN_TODO get mod version automatically
            GameVersion: new Version(major: 1, minor: 2, build: 8,
                                     revision: 0), // PLUGIN_TODO get game version automatically
            OutPath: modDir);
        builder.Prepare();

        string clientPath = Path.Combine(modDir, "Client");
        Directory.CreateDirectory(clientPath);

        string clientProjectPath = Path.Combine(projectRoot, "ClientSource", $"{prefix}Client.csproj");
        DotnetCmd.CompileProject(
            projectPath: clientProjectPath,
            configuration: Configuration.Debug,
            runtime: Runtime.Windows,
            outPath: clientPath);

        string clientAssemblyName =
            $"{Csproj.GetAssemblyName(Csproj.ParseCsproj(clientProjectPath)) ?? $"{prefix}Client"}.dll";

        builder.AddAssembly(Path.Combine(clientPath, clientAssemblyName));

        string serverPath = Path.Combine(modDir, "Server");
        string serverProjectPath = Path.Combine(projectRoot, "ServerSource", $"{prefix}Server.csproj");
        Directory.CreateDirectory(serverPath);
        DotnetCmd.CompileProject(
            projectPath: serverProjectPath,
            configuration: Configuration.Debug,
            runtime: Runtime.Windows,
            outPath: serverPath);

        string serverAssemblyName =
            $"{Csproj.GetAssemblyName(Csproj.ParseCsproj(serverProjectPath)) ?? $"{prefix}Server"}.dll";

        builder.AddAssembly(Path.Combine(serverPath, serverAssemblyName));
        builder.Build();
    }

    private static string? AskForInput(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine();
    }
}