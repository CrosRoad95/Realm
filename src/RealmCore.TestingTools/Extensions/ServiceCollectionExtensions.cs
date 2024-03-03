namespace RealmCore.TestingTools.Extensions;

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddRealmTestingServices(this ServiceCollection services, bool integrationTests)
    {
        if (!integrationTests)
        {
            services.AddScoped<IDb, NullDb>();
            services.AddScoped<IUserEventRepository>(x => null);
        }
        services.AddSingleton<TestingBrowserService>();
        services.AddSingleton<IBrowserService>(x => x.GetRequiredService<TestingBrowserService>());
        return services;
    }
}
