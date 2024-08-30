[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence<T>(this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbOptions) where T : DbContext, IDb
    {
        services.AddScoped<VehicleRepository>();
        services.AddScoped<GroupRepository>();
        services.AddScoped<TimeBaseOperationRepository>();
        services.AddScoped<BanRepository>();
        services.AddScoped<JobRepository>();
        services.AddScoped<UserRewardRepository>();
        services.AddScoped<UserNotificationRepository>();
        services.AddScoped<UserLoginHistoryRepository>();
        services.AddScoped<DataEventRepository>();
        services.AddScoped<RatingRepository>();
        services.AddScoped<OpinionRepository>();
        services.AddScoped<UserWhitelistedSerialsRepository>();
        services.AddScoped<NewsRepository>();
        services.AddScoped<InventoryRepository>();
        services.AddScoped<FriendRepository>();
        services.AddScoped<UsersRepository>();
        services.AddScoped<WorldNodeRepository>();
        services.AddScoped<UploadedFilesRepository>();
        services.AddScoped<ITransactionContext, TransactionContext>();

        services.AddDbContext<IDb, T>(dbOptions);

        return services;
    }

    public static IServiceCollection AddRealmIdentity<T>(this IServiceCollection services, IdentityConfiguration configuration) where T : Db<T>, IDb
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