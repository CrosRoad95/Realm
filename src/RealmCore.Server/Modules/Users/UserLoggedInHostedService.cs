namespace RealmCore.Server.Modules.Users;

internal sealed class UserLoggedInHostedService : PlayerLifecycle, IHostedService
{
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserLoginHistoryRepository _userLoginHistoryRepository;
    private readonly ILogger<UserLoggedInHostedService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserLoggedInHostedService(PlayersEventManager playersEventManager, IServiceProvider serviceProvider, ILogger<UserLoggedInHostedService> logger, IDateTimeProvider dateTimeProvider) : base(playersEventManager)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userLoginHistoryRepository = _serviceProvider.GetRequiredService<UserLoginHistoryRepository>();
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task PlayerLoggedIn(PlayerUserFeature user, RealmPlayer player)
    {
        var serial = player.Client.GetSerial();
        await _userLoginHistoryRepository.Add(user.Id, _dateTimeProvider.Now, player.Client.IPAddress?.ToString() ?? "", serial);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serviceScope.Dispose();

        return Task.CompletedTask;
    }
}
