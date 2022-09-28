namespace Realm.Persistance;

public static class PersistanceExtensions
{
    public static IServiceCollection AddPersistance<T>(this IServiceCollection services,
    Action<DbContextOptionsBuilder> dboptions) where T : DbContext, IDb
    {
        services.AddDbContextPool<IDb, T>(dboptions);

        services.AddTransient<ITestRepository, TestRepository>();
        return services;
    }
}
