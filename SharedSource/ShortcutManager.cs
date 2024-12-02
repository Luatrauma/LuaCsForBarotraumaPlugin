using System.Xml.Linq;
using Barotrauma;

namespace ClientSource;

// NOTE: there's no reason to store shorctuts, we could just re-use Map.LocationConnections, this is just for show how to store more data to campaign save file
public class ShortcutManager
{
    private readonly List<Shortcut> shortcuts = new();

    public readonly CampaignMode Campaign;

    public ShortcutManager(CampaignMode campaign, XElement element) : this(campaign)
    {
        foreach (var subElement in element.Elements())
        {
            shortcuts.Add(new Shortcut(campaign, subElement));
        }
    }

    public ShortcutManager(CampaignMode campaign)
    {
        Campaign = campaign;
    }

    public void AddShortcut(Location from, Location to)
    {
        Map map = Campaign.Map;
        int fromIndex = map.Locations.IndexOf(from);
        int toIndex = map.Locations.IndexOf(to);
        shortcuts.Add(new Shortcut(Campaign, fromIndex, toIndex));

        LocationConnection newConn = new LocationConnection(from, to)
        {
            Biome = from.Biome,
            LevelData = LevelData.CreateRandom("not_random_lol", 0, requireOutpost: false),
        };

        // reduce the length
        newConn.Length *= 0.25f;
        map.Connections.Add(newConn);
        from.Connections.Add(newConn);
        to.Connections.Add(newConn);
    }

    public void Save(XElement element)
    {
        foreach (var shortcut in shortcuts)
        {
            element.Add(shortcut.Save());
        }
    }

    public bool HasShortcut(Location mapCurrentLocation, Location hoveredLocation)
    {
        return shortcuts.Any(s => s.FromLocation == mapCurrentLocation && s.ToLocation == hoveredLocation);
    }
}

public readonly struct Shortcut
{
    public readonly int FromIndex;
    public readonly int ToIndex;

    public readonly Location FromLocation;
    public readonly Location ToLocation;

    public Shortcut(CampaignMode campaign, XElement element)
    {
        FromIndex = element.GetAttributeInt("fromindex", 0);
        ToIndex = element.GetAttributeInt("toindex", 0);

        FromLocation = campaign.Map.Locations[FromIndex];
        ToLocation = campaign.Map.Locations[ToIndex];
    }

    public Shortcut(CampaignMode campaign, int fromIndex, int toIndex)
    {
        FromIndex = fromIndex;
        ToIndex = toIndex;

        FromLocation = campaign.Map.Locations[fromIndex];
        ToLocation = campaign.Map.Locations[toIndex];
    }

    public XElement Save()
    {
        return new XElement(nameof(Shortcut),
                            new XAttribute("fromindex", FromIndex),
                            new XAttribute("toindex", ToIndex));
    }
}