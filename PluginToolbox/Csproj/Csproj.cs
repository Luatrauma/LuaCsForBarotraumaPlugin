using System.Xml.Linq;

namespace PluginToolbox;

public static class Csproj
{
    public static XDocument ParseCsproj(string path)
        => XDocument.Load(path);

    public static void SaveCsproj(XDocument doc, string path)
        => doc.Save(path);

    private static void SetPropertyGroup(XDocument doc, string elementName, string value)
    {
        if (doc.Root is null) { throw new Exception("Root element is null"); }

        XElement propertyGroupElement = doc.Root.Element("PropertyGroup") ?? throw new Exception("Invalid csproj file");

        XElement? existingElement = propertyGroupElement.Element(elementName);
        if (existingElement is not null)
        {
            existingElement.Value = value;
        }
        else
        {
            propertyGroupElement.Add(new XElement(elementName, value));
        }
    }

    public static void SetAssemblyName(XDocument doc, string assemblyName)
        => SetPropertyGroup(doc, "AssemblyName", assemblyName);

    public static void SetVersion(XDocument doc, Version version)
        => SetPropertyGroup(doc, "Version", version.ToString());

    public static void SetRootNamespace(XDocument doc, string rootNamespace)
        => SetPropertyGroup(doc, "RootNamespace", rootNamespace);

    public static void SetAuthors(XDocument doc, string authors)
        => SetPropertyGroup(doc, "Authors", authors);

    public static void SetRepositoryUrl(XDocument doc, string repositoryUrl)
        => SetPropertyGroup(doc, "RepositoryUrl", repositoryUrl);

    public static void SetBaroMetadata(XDocument doc, params Metadata[] metadata)
    {
        if (doc.Root is null) { throw new Exception("Root element is null"); }

        const string elementName = "BarotraumaMetadata";

        XElement? itemGroup = doc
                              .Elements()
                              .FirstOrDefault(static element =>
                                                  element.Name == "ItemGroup" &&
                                                  element.Attribute("Label")?.Value == elementName);

        if (itemGroup is null)
        {
            XElement newItemGroup = new("ItemGroup", new XAttribute("Label", elementName));
            doc.Root.Add(newItemGroup);
            itemGroup = newItemGroup;
        }

        foreach (Metadata data in metadata)
        {
            itemGroup
                .Elements()
                .Where(e => e.Element("_Parameter1")?.Value == data.Key)
                .Remove(); // Remove existing metadata with the same key

            XElement assemblyAttribute = new("AssemblyAttribute",
                                             new XAttribute("Include", "System.Reflection.AssemblyMetadataAttribute"),
                                             new XElement("_Parameter1", data.Key),
                                             new XElement("_Parameter2", data.Value));
            itemGroup.Add(assemblyAttribute);
        }
    }
}