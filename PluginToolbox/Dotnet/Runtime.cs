namespace PluginToolbox;

internal class Runtime
{
    public readonly string Name;
    public readonly string Identifier;

    private Runtime(string name, string identifier)
    {
        Name = name;
        Identifier = identifier;
    }

    public static readonly Runtime Windows = new("Windows", "win-x64");
    public static readonly Runtime Mac = new("Mac", "osx-x64");
    public static readonly Runtime Linux = new("Linux", "linux-x64");
}