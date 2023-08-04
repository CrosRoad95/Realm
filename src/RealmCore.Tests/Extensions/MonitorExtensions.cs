using FluentAssertions.Events;
using RealmCore.Tests.Classes;

namespace RealmCore.Tests.Extensions;

internal static class MonitorExtensions
{
    public static IEnumerable<SimpleEventInfo> GetEvents<T>(this IMonitor<T> monitor, string prefix)
    {
        return monitor.OccurredEvents.Select(x => new SimpleEventInfo
         {
             TimestampUtc = x.TimestampUtc,
             Name = prefix + "/" + x.EventName
         });
    }
}
