using System.Xml.Linq;

namespace PluginToolbox;

internal record ContentPackageBuilder(string ModName, Version ModVersion, Version GameVersion, string OutPath)
{
    private readonly List<string> fullAssemblyPaths = new();

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

    public void AddAssembly(string assemblyPath)
    {
        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException("Assembly not found", assemblyPath);
        }

        fullAssemblyPaths.Add(assemblyPath);
    }

    private IEnumerable<string> GetLocalAssemblyPaths()
        => fullAssemblyPaths.Select(path => Path.GetRelativePath(OutPath, path));

    private XDocument CreateFileList()
    {
        XElement root = new("contentpackage",
                            new XAttribute("name", ModName),
                            new XAttribute("modversion", ModVersion),
                            new XAttribute("corepackage", false),
                            new XAttribute("gameversion", GameVersion));

        foreach (string path in GetLocalAssemblyPaths())
        {
            XElement pluginFile = new("Plugin",
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