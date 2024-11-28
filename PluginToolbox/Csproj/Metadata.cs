namespace PluginToolbox;

public class Metadata
{
    public readonly string Key;
    public readonly string Value;

    private Metadata(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public static Metadata GameVersion(Version value)
        => new("GameVersion", value.ToString());

    public static Metadata RepositoryUrl(string value)
        => new("RepositoryUrl", value);
}