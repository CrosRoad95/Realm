using Realm.Persistance.Data;
using Realm.Persistance.Interfaces;

namespace Realm.Persistance;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistance<T>(this IServiceCollection services,
    Action<DbContextOptionsBuilder> dboptions) where T : DbContext, IDb
    {
        services.AddDbContextPool<IDb, T>(dboptions);

        services.AddTransient<ITestRepository, TestRepository>();
        return services;
    }

    public static IServiceCollection AddRealmIdentity<T>(this IServiceCollection services) where T : Db<T>
    {
        services.AddIdentity<User, Role>(setup =>
        {
            setup.SignIn.RequireConfirmedAccount = true;
        })
           .AddEntityFrameworkStores<T>()
           .AddDefaultTokenProviders();
        return services;
    }
}