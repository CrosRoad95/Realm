﻿using RealmCore.Persistence.Repository;

namespace RealmCore.Persistence;

public class AuthorizationPoliciesProvider
{
    private readonly HashSet<string> _policies;
    public AuthorizationPoliciesProvider(IEnumerable<string> policies)
    {
        _policies = new HashSet<string>(policies);
    }

    public void ValidatePolicy(string policy)
    {
        if (!_policies.Contains(policy))
            throw new NotSupportedException($"Not supported policy '{policy}'");
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence<T>(this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbOptions, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : DbContext, IDb
    {
        services.AddSingleton<RealmDbContextFactory>();
        services.AddSingleton<RepositoryFactory>();

        services.AddTransient<IVehicleRepository, VehicleRepository>();
        services.AddTransient<IGroupRepository, GroupRepository>();
        services.AddTransient<IFractionRepository, FractionRepository>();
        services.AddTransient<IBanRepository, BanRepository>();
        services.AddTransient<IJobRepository, JobRepository>();
        services.AddTransient<IUserRewardRepository, UserRewardRepository>();
        services.AddTransient<IVehicleEventRepository, VehicleEventRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IRatingRepository, RatingRepository>();
        services.AddTransient<IOpinionRepository, OpinionRepository>();

        services.AddDbContext<IDb, T>(dbOptions, serviceLifetime);

        return services;
    }

    public static IServiceCollection AddPersistence<T>(this IServiceCollection services) where T : DbContext, IDb
    {
        services.AddDbContext<IDb, T>(ServiceLifetime.Transient);

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