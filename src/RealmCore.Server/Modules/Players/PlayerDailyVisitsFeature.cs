namespace RealmCore.Server.Modules.Players;

public interface IPlayerDailyVisitsFeature : IPlayerFeature
{
    DateTime LastVisit { get; set; }
    int VisitsInRow { get; set; }
    int VisitsInRowRecord { get; set; }

    event Action<IPlayerDailyVisitsFeature, int, bool>? Visited;
    event Action<IPlayerDailyVisitsFeature, int>? VisitsRecord;

    internal void Update(DateTime now);
}

internal sealed class PlayerDailyVisitsFeature : IPlayerDailyVisitsFeature, IDisposable
{
    private readonly object _lock = new();
    private readonly IPlayerUserFeature _playerUserFeature;
    private DailyVisitsData? _dailyVisitsData;
    public DateTime LastVisit
    {
        get => _dailyVisitsData?.LastVisit ?? throw new InvalidOperationException(); set
        {
            if (_dailyVisitsData == null)
                throw new InvalidOperationException();
            _dailyVisitsData.LastVisit = value;
        }
    }
    public int VisitsInRow
    {
        get => _dailyVisitsData?.VisitsInRow ?? throw new InvalidOperationException(); set
        {
            if (_dailyVisitsData == null)
                throw new InvalidOperationException();
            _dailyVisitsData.VisitsInRow = value;
        }
    }
    public int VisitsInRowRecord
    {
        get => _dailyVisitsData?.VisitsInRowRecord ?? throw new InvalidOperationException(); set
        {
            if (_dailyVisitsData == null)
                throw new InvalidOperationException();
            _dailyVisitsData.VisitsInRowRecord = value;
        }
    }

    public event Action<IPlayerDailyVisitsFeature, int, bool>? Visited;
    public event Action<IPlayerDailyVisitsFeature, int>? VisitsRecord;

    public RealmPlayer Player { get; init; }
    public PlayerDailyVisitsFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer player)
    {
        var now = player.GetRequiredService<IDateTimeProvider>().Now;
        lock (_lock)
        {
            if (playerUserFeature.User?.DailyVisits != null)
            {
                _dailyVisitsData = playerUserFeature.User?.DailyVisits;
            }
            else
            {
                _dailyVisitsData = new DailyVisitsData
                {
                    LastVisit = now,
                    VisitsInRow = 0,
                    VisitsInRowRecord = 0,
                };
            }
        }
        Update(now);
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _dailyVisitsData = null;
    }

    public void Update(DateTime now)
    {
        var nowDate = now.Date;
        if (LastVisit.Date == nowDate)
            return;

        bool reset = false;

        if (LastVisit.Date.AddDays(1) == nowDate || LastVisit == DateTime.MinValue)
        {
            VisitsInRow++;
        }
        else
        {
            VisitsInRow = 0;
            reset = true;
        }

        if (VisitsInRow > VisitsInRowRecord) // Doesn't check if day passed because value can be arbitrarily changed
        {
            VisitsInRowRecord = VisitsInRow;
            VisitsRecord?.Invoke(this, VisitsInRowRecord);
        }

        _playerUserFeature.IncreaseVersion();
        Visited?.Invoke(this, VisitsInRow, reset);
        LastVisit = nowDate;
    }

    public void Dispose()
    {

    }
}
