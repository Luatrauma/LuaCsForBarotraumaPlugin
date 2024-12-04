using System.Xml.Linq;

namespace PluginToolbox;

internal record ContentPackageBuilder(string ModName, Version ModVersion, Version GameVersion, string OutPath)
{
    public enum AssemblyType
    {
        Server,
        Client
    }

    private readonly Dictionary<AssemblyType, string> fullAssemblyPaths = new();

    public void Prepare()
    {
        if (!Directory.Exists(OutPath))
        {
            Directory.CreateDirectory(OutPath);
        }
        else
        {
            Directory.Delete(OutPath, recursive: true);
            Directory.CreateDirectory(OutPath);
        }
    }

    public void AddAssembly(AssemblyType type, string assemblyPath)
    {
        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException("Assembly not found", assemblyPath);
        }

        fullAssemblyPaths.Add(type, assemblyPath);
    }

    private Dictionary<AssemblyType, string> GetLocalAssemblyPaths()
        => fullAssemblyPaths.ToDictionary(static kvp => kvp.Key, kvp => Path.GetRelativePath(OutPath, kvp.Value));

    private XDocument CreateFileList()
    {
        XElement root = new("contentpackage",
                            new XAttribute("name", ModName),
                            new XAttribute("modversion", ModVersion),
                            new XAttribute("corepackage", false),
                            new XAttribute("gameversion", GameVersion));

        foreach (var (type, path) in GetLocalAssemblyPaths())
        {
            XElement pluginFile = new(type switch
                                      {
                                            AssemblyType.Server => "ServerPlugin",
                                            AssemblyType.Client => "ClientPlugin",
                                            _ => throw new ArgumentOutOfRangeException(nameof(type))
                                      },
                                      new XAttribute("file", $"%ModDir%/{path.Replace("\\", "/")}"));
            root.Add(pluginFile);
        }

        XDocument doc = new();
        doc.Add(root);
        return doc;
    }

    public void Build()
    {
        XDocument fileList = CreateFileList();
        fileList.Save(Path.Combine(OutPath, "filelist.xml"));
    }
}