using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Order the config options in a menu, lower values appears first.
/// ConfigEntryOrder (x, y) x is catagory, y is name/order of section.
/// </summary>
/// <example>
/// (entry 1 and 2 of the first section and the first entry of section 2)
/// Config.Bind("General", "Enabled", true, new ConfigDescription("Turns the option on or off.", null, new ConfigEntryOrder(0, 0)));
/// Config.Bind("General", "Speed", 1f, new ConfigDescription("Speed value.", null, new ConfigEntryOrder(0, 1)));
/// Config.Bind("Debug", "Logging", false, new ConfigDescription("Logs a specific feature.", null, new ConfigEntryOrder(1, 0)));
/// </example>
public record ConfigEntryOrder
{
    /// <summary>
    /// Gets the display position of the section. Lower values appear first.
    /// </summary>
    public int Section { get; }

    /// <summary>
    /// Gets the display position of the entry within its section. Lower values appear first.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Initializes ordering information for a configuration entries.
    /// </summary>
    public ConfigEntryOrder(int section, int order)
    {
        Section = section;
        Order = order;
    }

    internal static ConfigEntryOrder? Of(ConfigEntryBase entry) =>
        entry.Description?.Tags?.OfType<ConfigEntryOrder>().FirstOrDefault();

    internal static Dictionary<string, int>? SectionRanks(ConfigFile config)
    {
        if (!config.Any(pair => Of(pair.Value) != null))
            return null;

        return config
            .GroupBy(pair => pair.Key.Section)
            .Select(section =>
                (
                    Name: section.Key,
                    Position: section.Min(pair => Of(pair.Value)?.Section ?? int.MaxValue)
                )
            )
            .OrderBy(section => section.Position)
            .ThenBy(section => section.Name)
            .Select((section, rank) => (section.Name, Rank: rank))
            .ToDictionary(section => section.Name, section => section.Rank);
    }
}
