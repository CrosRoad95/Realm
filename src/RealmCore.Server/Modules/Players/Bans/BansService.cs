namespace RealmCore.Server.Modules.Players.Bans;

public interface IBansService
{
    Task<BanDto[]> GetBySerial(string serial, int? type = null, CancellationToken cancellationToken = default);
}

internal sealed class BansService : IBansService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private readonly IBanRepository _banRepository;
    private readonly IServiceScope _serviceScope;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BansService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _banRepository = _serviceScope.ServiceProvider.GetRequiredService<IBanRepository>();
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<BanDto[]> GetBySerial(string serial, int? type = null, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var bans = await _banRepository.GetBySerial(serial, _dateTimeProvider.Now, type, cancellationToken);
            return bans.Select(BanDto.Map).ToArray();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
