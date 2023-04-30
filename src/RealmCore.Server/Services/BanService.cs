namespace RealmCore.Server.Services;

public class BanService : IBanService
{
    private readonly IBanRepository _banRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<BanData>? BanAdded;
    public event Action<BanData>? BanRemoved;
    public BanService(IBanRepository banRepository, IDateTimeProvider dateTimeProvider)
    {
        _banRepository = banRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task BanSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        var banData = _banRepository.CreateBanForSerial(serial, until, reason, responsible, type);
        if (await _banRepository.Commit() > 0)
            BanAdded?.Invoke(banData);

    }

    public async Task BanUserId(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        _banRepository.CreateBanForUser(userId, until, reason, responsible, type);
        await _banRepository.Commit();
    }

    public async Task RemoveBan(BanData ban)
    {
        _banRepository.RemoveBan(ban);
        await _banRepository.Commit();
        BanRemoved?.Invoke(ban);
    }

    public Task<List<BanData>> GetBansBySerial(string serial) => _banRepository.GetBansBySerial(serial, _dateTimeProvider.Now);

    public Task<List<BanData>> GetBansByUserId(int userId) => _banRepository.GetBansByUserId(userId, _dateTimeProvider.Now);

    public Task<BanData?> GetBanBySerialAndBanType(string serial, int banType) => _banRepository.GetBanBySerialAndBanType(serial, banType, _dateTimeProvider.Now);
}
