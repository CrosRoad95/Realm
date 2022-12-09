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
}
