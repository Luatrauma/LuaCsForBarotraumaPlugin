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

    public static Metadata Identifier(string value)
        => new("Identifier", value);

    public static Metadata ApiLevel(int value)
        => new("ApiLevel", value.ToString());

    public static Metadata GameVersion(string value)
        => new("GameVersion", value);
}