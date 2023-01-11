namespace Realm.Persistance.Extensions;

public static class QuerableExtensions
{
    public static IQueryable<UserLicense> NotSuspended(this IQueryable<UserLicense> query)
    {
        return query.Where(x => x.SuspendedUntil == null || x.SuspendedUntil < DateTime.Now);
    }

    public static IQueryable<UserLicense> IsSuspended(this IQueryable<UserLicense> query)
    {
        return query.Where(x => x.SuspendedUntil != null && x.SuspendedUntil > DateTime.Now);
    }

    public static IQueryable<Vehicle> IsNotRemoved(this IQueryable<Vehicle> query)
    {
        return query.Where(x => !x.Removed);
    }

    public static IQueryable<User> IncludeAll(this IQueryable<User> query)
    {
        return query.Include(x => x.Licenses)
            .Include(x => x.JobUpgrades)
            .Include(x => x.Achievements)
            .Include(x => x.DailyVisits)
            .Include(x => x.Statistics)
            .Include(x => x.Discoveries)
            .Include(x => x.Inventories)
            .ThenInclude(x => x!.InventoryItems);
    }

    public static IQueryable<Vehicle> IncludeAll(this IQueryable<Vehicle> query)
    {
        return query.Include(x => x.Fuels)
            .Include(x => x.Upgrades)
            .Include(x => x.VehicleAccesses)
            .ThenInclude(x => x.User);
    }
}
