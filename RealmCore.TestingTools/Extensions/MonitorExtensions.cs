using FluentAssertions.Events;
using RealmCore.TestingTools.Classes;

namespace RealmCore.TestingTools.Extensions;

public static class MonitorExtensions
{
    public static IEnumerable<SimpleEventInfo> GetEvents<T>(this IMonitor<T> monitor, string prefix)
    {
        return monitor.OccurredEvents.Select(x => new SimpleEventInfo
        {
            TimestampUtc = x.TimestampUtc,
            Name = prefix + "/" + x.EventName
        });
    }

    public static IEnumerable<string> GetOccurredEvents<T>(this IMonitor<T> monitor)
    {
        return monitor.OccurredEvents
            .OrderBy(x => x.TimestampUtc)
            .Select(x => x.EventName);
    }
}
