﻿namespace RealmCore.Server.Modules.Users;

internal sealed class UserLoggedInHostedService : PlayerLifecycle, IHostedService
{
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserLoginHistoryRepository _userLoginHistoryRepository;
    private readonly ILogger<UserLoggedInHostedService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserLoggedInHostedService(PlayersEventManager playersEventManager, IServiceProvider serviceProvider, ILogger<UserLoggedInHostedService> logger, IDateTimeProvider dateTimeProvider) : base(playersEventManager)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userLoginHistoryRepository = _serviceProvider.GetRequiredService<IUserLoginHistoryRepository>();
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    private async Task PlayerSignedInCore(IPlayerUserFeature user, RealmPlayer player)
    {
        var serial = player.Client.GetSerial();
        await _userLoginHistoryRepository.Add(user.Id, _dateTimeProvider.Now, player.Client.IPAddress?.ToString() ?? "", serial);
    }

    protected override async void PlayerLoggedIn(IPlayerUserFeature user, RealmPlayer player)
    {
        try
        {
            await PlayerSignedInCore(user, player);
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
        }
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