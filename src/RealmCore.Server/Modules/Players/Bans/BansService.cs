namespace RealmCore.Server.Modules.Players.Bans;

public sealed class BansService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly BanRepository _banRepository;
    private readonly IServiceScope _serviceScope;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BansService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _banRepository = _serviceScope.ServiceProvider.GetRequiredService<BanRepository>();
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<UserBanDto[]> GetBySerial(string serial, int? type = null, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var bans = await _banRepository.GetBySerial(serial, _dateTimeProvider.Now, type, cancellationToken);
            return bans.Select(UserBanDto.Map).ToArray();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
