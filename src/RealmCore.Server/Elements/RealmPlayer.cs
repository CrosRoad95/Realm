namespace RealmCore.Server.Elements;

public class RealmPlayer : Player, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;

    private CancellationTokenSource? _cancellationTokenSource;
    protected CancellationToken CancellationToken => (_cancellationTokenSource ??= new()).Token;
    public IServiceProvider ServiceProvider => _serviceProvider;

    public RealmPlayer(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
