using Realm.Common.Providers;

namespace Realm.Persistance.Extensions;

public static class QuerableExtensions
{
    public static IQueryable<UserLicense> NotSuspended(this IQueryable<UserLicense> query, IDateTimeProvider dateTimeProvider)
    {
        return query.Where(x => x.SuspendedUntil == null || x.SuspendedUntil < dateTimeProvider.Now);
    }

    public static IQueryable<UserLicense> IsSuspended(this IQueryable<UserLicense> query, IDateTimeProvider dateTimeProvider)
    {
        return query.Where(x => x.SuspendedUntil != null && x.SuspendedUntil > dateTimeProvider.Now);
    }

    public static IQueryable<Vehicle> IsNotRemoved(this IQueryable<Vehicle> query)
    {
        return query.Where(x => !x.Removed);
    }

    public static IQueryable<User> IncludeAll(this IQueryable<User> query)
    {
        return query
            .AsSplitQuery()
            .Include(x => x.Licenses)
            .Include(x => x.JobUpgrades)
            .Include(x => x.JobStatistics)
            .Include(x => x.Achievements)
            .Include(x => x.DailyVisits)
            .Include(x => x.Stats)
            .Include(x => x.Discoveries)
            .Include(x => x.GroupMembers)
            .Include(x => x.FractionMembers)
            .Include(x => x.DiscordIntegration)
            .Include(x => x.Upgrades)
            .Include(x => x.Settings)
            .Include(x => x.Inventories)
            .ThenInclude(x => x!.InventoryItems);
    }

    public static IQueryable<Vehicle> IncludeAll(this IQueryable<Vehicle> query)
    {
        return query.Include(x => x.Fuels)
            .Include(x => x.Upgrades)
            .Include(x => x.PartDamages)
            .Include(x => x.VehicleEngines)
            .Include(x => x.Inventories)
            .ThenInclude(x => x!.InventoryItems)
            .Include(x => x.PlayerAccesses)
            .ThenInclude(x => x.User);
    }

    public static IQueryable<Vehicle> IsSpawned(this IQueryable<Vehicle> query)
    {
        return query.Where(x => x.Spawned);
    }

    public static IQueryable<T> TagWithSource<T>(this IQueryable<T> queryable, string tag = "", [CallerMemberName] string methodName = "")
    {
        return queryable.TagWith(string.IsNullOrEmpty(tag) ? $"{methodName}" : $"{tag}{methodName}");
    }
}
