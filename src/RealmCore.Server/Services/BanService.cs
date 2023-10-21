namespace RealmCore.Server.Services;

internal sealed class BanService : IBanService
{
    private readonly IBanRepository _banRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BanService(IBanRepository banRepository, IDateTimeProvider dateTimeProvider)
    {
        _banRepository = banRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task BanAccount(Entity entity, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        if (entity.TryGetComponent(out UserComponent userComponent))
            await _banRepository.CreateBanForUser(userComponent.Id, until, reason, responsible, type);
    }

    public async Task BanPlayer(Entity entity, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            var serial = playerElementComponent.Client.Serial ?? throw new InvalidOperationException();
            await _banRepository.CreateBanForSerial(serial, until, reason, responsible, type);
        }
    }
    
    public async Task Ban(Entity entity, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        await BanAccount(entity, until, reason, responsible, type);
        await BanPlayer(entity, until, reason, responsible, type);
    }

    public async Task<bool> RemoveBan(Entity entity, int type = 0)
    {
        if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            var serial = playerElementComponent.Client.Serial ?? throw new InvalidOperationException();
            await _banRepository.DeleteBySerial(serial, type);
            if (entity.TryGetComponent(out UserComponent userComponent))
            {
                await _banRepository.DeleteByUserId(userComponent.Id, type);
            }
            return true;
        }
        return false;
    }

    public async Task<List<BanData>> GetBans(Entity entity)
    {
        if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            var serial = playerElementComponent.Client.Serial ?? throw new InvalidOperationException();
            if (entity.TryGetComponent(out UserComponent userComponent))
            {
                return await _banRepository.GetBansByUserIdOrSerial(userComponent.Id, serial, _dateTimeProvider.Now);
            }
            else
            {
                return await _banRepository.GetBansBySerial(serial, _dateTimeProvider.Now);
            }
        }

        return new();
    }
}
