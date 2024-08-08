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
            .ThenInclude(x => x.Group)
            .Include(x => x.GroupMembers)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x.Permissions)
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
            .Include(x => x.Boosts)
            .Include(x => x.ActiveBoosts)
            .Include(x => x.Secrets)
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

    public static IQueryable<GroupData> IncludeAll(this IQueryable<GroupData> query)
    {
        return query
            .Include(x => x.Members)
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
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

    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, QueryPage page)
    {
        if (page.page <= 0)
        {
            throw new ArgumentException("Page number must be greater than 0.", nameof(page.page));
        }

        if (page.limit <= 0)
        {
            throw new ArgumentException("Page size must be greater than 0.", nameof(page.limit));
        }

        return query.Skip((page.page - 1) * page.limit).Take(page.limit);
    }

    public static IQueryable<T> InDateTimeRange<T>(this IQueryable<T> query, DateRange dateRange) where T: EventDataBase
    {
        if(dateRange.from >= dateRange.to)
            throw new ArgumentException("Invalid dateRange", nameof(dateRange));

        return query.Where(x => x.DateTime >= dateRange.from && x.DateTime <= dateRange.to);
    }
}
