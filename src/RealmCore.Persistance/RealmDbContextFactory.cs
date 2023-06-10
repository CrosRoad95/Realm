namespace RealmCore.Persistence;

public class RealmDbContextFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RealmDbContextFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDb CreateDbContext() => _serviceProvider.GetRequiredService<IDb>();
}
