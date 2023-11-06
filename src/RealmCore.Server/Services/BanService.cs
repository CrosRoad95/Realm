using RealmCore.Server.DomainObjects;

namespace RealmCore.Server.Services;

internal sealed class BanService : IBanService
{
    private readonly IBanRepository _banRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<BanDTO>? Banned;
    public event Action<string, int, int?>? SerialUnbanned;
    public event Action<int, int, int?>? UserUnbanned;

    public BanService(IBanRepository banRepository, IDateTimeProvider dateTimeProvider)
    {
        _banRepository = banRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<BanDTO> BanUser(RealmPlayer player, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        return Map(await _banRepository.CreateBanForUser(player.UserId, until, reason, responsible, type, cancellationToken));
    }

    public async Task<BanDTO> BanPlayer(RealmPlayer player, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        var serial = player.Client.GetSerial();
        return Map(await _banRepository.CreateBanForSerial(serial, until, reason, responsible, type, cancellationToken));
    }

    public async Task<BanDTO> Ban(RealmPlayer player, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        var serial = player.Client.GetSerial();
        var banDTO = Map(await _banRepository.CreateBanForUserIdAndSerial(player.UserId, serial, until, reason, responsible, type, cancellationToken));
        Banned?.Invoke(banDTO);
        return banDTO;
    }

    public async Task RemoveBan(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default)
    {
        var serial = player.Client.GetSerial();
        if (player.IsSignedIn)
        {
            var deletedBansIds = await _banRepository.DeleteByUserIdOrSerial(player.UserId, serial, type, cancellationToken);
            foreach (var banId in deletedBansIds)
            {
                SerialUnbanned?.Invoke(serial, banId, type);
                UserUnbanned?.Invoke(player.UserId, banId, type); // TODO: Don't call serial/user if ban doesn't existed
            }
        }
        else
        {
            var deletedBansIds = await _banRepository.DeleteBySerial(serial, type);
            foreach (var banId in deletedBansIds)
            {
                SerialUnbanned?.Invoke(serial, banId, type);
            }
        }
    }

    public async Task<Bans> GetBans(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default)
    {
        List<BanData> bansData;

        var serial = player.Client.GetSerial();
        if (player.IsSignedIn)
        {
            bansData = await _banRepository.GetBansByUserIdOrSerial(player.UserId, serial, _dateTimeProvider.Now, type, cancellationToken);
        }
        else
        {
            bansData = await _banRepository.GetBansBySerial(serial, _dateTimeProvider.Now, type, cancellationToken);
        }

        return new(bansData.Select(Map).ToList());
    }

    public async Task<Bans> GetBansByUserIdAndSerial(int userId, string serial, int? type = null, CancellationToken cancellationToken = default)
    {
        List<BanData> bansData = await _banRepository.GetBansByUserIdOrSerial(userId, serial, _dateTimeProvider.Now, type, cancellationToken);

        return new(bansData.Select(Map).ToList());
    }

    public async Task<Bans> GetBansByUserId(int userId, int? type = null, CancellationToken cancellationToken = default)
    {
        List<BanData> bansData = await _banRepository.GetBansByUserId(userId, _dateTimeProvider.Now, type, cancellationToken);

        return new(bansData.Select(Map).ToList());
    }

    public async Task<Bans> GetBansBySerial(string serial, int? type = null, CancellationToken cancellationToken = default)
    {
        List<BanData> bansData = await _banRepository.GetBansBySerial(serial, _dateTimeProvider.Now, type, cancellationToken);

        return new(bansData.Select(Map).ToList());
    }

    public async Task<bool> IsBanned(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default)
    {
        var bans = await _banRepository.GetBansByUserId(player.UserId, _dateTimeProvider.Now, type, cancellationToken);
        return bans.Count > 0;
    }

    private static BanDTO Map(BanData banData) => new()
    {
        Id = banData.Id,
        End = banData.End,
        UserId = banData.UserId,
        Reason = banData.Reason,
        Responsible = banData.Responsible,
        Serial = banData.Serial,
        Type = banData.Type
    };
}
