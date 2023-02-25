using Realm.Persistance.Interfaces;

namespace Realm.Server.Services;

public class BanService : IBanService
{
    private readonly IBanRepository _banRepository;

    public event Action<Ban>? BanAdded;
    public event Action<Ban>? BanRemoved;
    public BanService(IBanRepository banRepository)
    {
        _banRepository = banRepository;
    }

    public async Task BanSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        _banRepository.CreateBanForSerial(serial, until, reason, responsible, type);
        await _banRepository.Commit();
    }

    public async Task BanUserId(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        _banRepository.CreateBanForUser(userId, until, reason, responsible, type);
        await _banRepository.Commit();
    }

    public async Task RemoveBan(Ban ban)
    {
        _banRepository.RemoveBan(ban);
        await _banRepository.Commit();
    }

    public Task<List<Ban>> GetBansBySerial(string serial)
    {
        return _banRepository.GetBansBySerial(serial);
    }

    public Task<List<Ban>> GetBansByUserId(int userId)
    {
        return _banRepository.GetBansByUserId(userId);
    }
}
