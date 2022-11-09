namespace Realm.Persistance;

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
            throw new Exception($"Not supported policy '{policy}'");
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistance<T>(this IServiceCollection services,
    Action<DbContextOptionsBuilder> dboptions) where T : DbContext, IDb
    {
        services.AddDbContext<IDb, T>(dboptions, ServiceLifetime.Transient);

        return services;
    }

    public static IServiceCollection AddRealmIdentity<T>(this IServiceCollection services, IdentityConfiguration configuration) where T : Db<T>
    {
        services.AddIdentity<User, Role>(setup =>
        {
            setup.SignIn.RequireConfirmedAccount = true;
        })
           .AddEntityFrameworkStores<T>()
           .AddDefaultTokenProviders();

        services.AddSingleton(new AuthorizationPoliciesProvider(configuration.Policies.Keys));
        services.AddTransient<PlayerAccount>();

        services.AddAuthorization(options =>
        {
            foreach (var identityPolicy in configuration.Policies)
            {
                options.AddPolicy(identityPolicy.Key, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(identityPolicy.Value.RequireRoles);
                    foreach (var claim in identityPolicy.Value.RequireClaims)
                        policy.RequireClaim(claim.Key, claim.Value);
                });
            }
        });

        return services;
    }
}