namespace RealmCore.Persistence.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<UserLicenseData> NotSuspended(this IQueryable<UserLicenseData> query, DateTime now)
    {
        return query.Where(x => x.SuspendedUntil == null || x.SuspendedUntil < now);
    }

    public static IQueryable<UserLicenseData> IsSuspended(this IQueryable<UserLicenseData> query, DateTime now)
    {
        return query.Where(x => x.SuspendedUntil != null && x.SuspendedUntil > now);
    }

    public static IQueryable<VehicleData> IsNotRemoved(this IQueryable<VehicleData> query)
    {
        return query.Where(x => !x.IsRemoved);
    }

    public static IQueryable<UserData> IncludeAll(this IQueryable<UserData> query, DateTime now)
    {
        return query
            .Include(x => x.Licenses)
            .Include(x => x.JobUpgrades)
            .Include(x => x.JobStatistics)
            .Include(x => x.Achievements)
            .Include(x => x.DailyVisits)
            .Include(x => x.Stats)
            .Include(x => x.GtaSaStats)
            .Include(x => x.Discoveries)
            .Include(x => x.GroupMembers)
            .Include(x => x.FractionMembers)
            .Include(x => x.DiscordIntegration)
            .Include(x => x.Upgrades)
            .Include(x => x.Settings)
            .Include(x => x.Events.OrderByDescending(x => x.Id).Take(10))
            .Include(x => x.WhitelistedSerials)
            .Include(x => x.Bans.Where(x => x.Active && x.End > now))
            .Include(x => x.Inventories)
            .ThenInclude(x => x!.InventoryItems)
            .Include(x => x.PlayTimes)
            .Include(x => x.BlockedUsers)
            .Include(x => x.DailyTasksProgress)
            .AsSplitQuery();
    }

    public static IQueryable<VehicleData> IncludeAll(this IQueryable<VehicleData> query)
    {
        return query.Include(x => x.Fuels)
            .Include(x => x.Upgrades)
            .Include(x => x.PartDamages)
            .Include(x => x.VehicleEngines)
            .Include(x => x.Inventories)
            .ThenInclude(x => x!.InventoryItems)
            .Include(x => x.UserAccesses)
            .AsSplitQuery();
    }

    public static IQueryable<VehicleData> IsSpawned(this IQueryable<VehicleData> query)
    {
        return query.Where(x => x.Spawned);
    }

    public static IQueryable<T> TagWithSource<T>(this IQueryable<T> queryable, string tag = "", [CallerMemberName] string methodName = "")
    {
        return queryable.TagWith(string.IsNullOrEmpty(tag) ? $"{methodName}" : $"{tag}{methodName}");
    }
}
