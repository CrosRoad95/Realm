[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence<T>(this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbOptions) where T : DbContext, IDb
    {
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IFractionRepository, FractionRepository>();
        services.AddScoped<IBanRepository, BanRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IUserRewardRepository, UserRewardRepository>();
        services.AddScoped<IVehicleEventRepository, VehicleEventRepository>();
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        services.AddScoped<IUserLoginHistoryRepository, UserLoginHistoryRepository>();
        services.AddScoped<IUserMoneyHistoryRepository, UserMoneyHistoryRepository>();
        services.AddScoped<IUserEventRepository, UserEventRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IOpinionRepository, OpinionRepository>();
        services.AddScoped<IUserWhitelistedSerialsRepository, UserWhitelistedSerialsRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<ITransactionContext, TransactionContext>();

        services.AddDbContext<IDb, T>(dbOptions);

        return services;
    }

    public static IServiceCollection AddRealmIdentity<T>(this IServiceCollection services, IdentityConfiguration configuration) where T : Db<T>
    {
        services.AddIdentity<UserData, RoleData>(setup =>
        {
            setup.SignIn.RequireConfirmedAccount = true;
        })
           .AddEntityFrameworkStores<T>()
           .AddDefaultTokenProviders()
           .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<UserData>>();

        services.AddSingleton(new AuthorizationPoliciesProvider(configuration.Policies.Keys));

        services.AddAuthorization(options =>
        {
            foreach (var identityPolicy in configuration.Policies)
            {
                options.AddPolicy(identityPolicy.Key, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(identityPolicy.Value.RequireRoles);
                    if(identityPolicy.Value.RequireClaims != null)
                        foreach (var claim in identityPolicy.Value.RequireClaims)
                            policy.RequireClaim(claim.Key, claim.Value);
                });
            }
        });

        return services;
    }
}