﻿namespace RealmCore.Server.Modules.Players.DailyVisits;

public sealed class PlayerDailyVisitsFeature : IPlayerFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private UserDailyVisitsData? _dailyVisitsData;

    public DateTime LastVisit
    {
        get => _dailyVisitsData?.LastVisit ?? DateTime.MinValue; set
        {
            if (_dailyVisitsData != null)
                _dailyVisitsData.LastVisit = value;
        }
    }

    public int VisitsInRow
    {
        get => _dailyVisitsData?.VisitsInRow ?? 0; set
        {
            if (_dailyVisitsData != null)
                _dailyVisitsData.VisitsInRow = value;
        }
    }

    public int VisitsInRowRecord
    {
        get => _dailyVisitsData?.VisitsInRowRecord ?? 0; set
        {
            if (_dailyVisitsData != null)
                _dailyVisitsData.VisitsInRowRecord = value;
        }
    }

    public event Action<PlayerDailyVisitsFeature, int, bool>? Visited;
    public event Action<PlayerDailyVisitsFeature, int>? VisitsRecord;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }

    public PlayerDailyVisitsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
        {
            if (userData.DailyVisits != null)
            {
                _dailyVisitsData = userData.DailyVisits;
            }
            else
            {
                _dailyVisitsData = new UserDailyVisitsData
                {
                    LastVisit = DateTime.MinValue,
                    VisitsInRow = 0,
                    VisitsInRowRecord = 0,
                };
                userData.DailyVisits = _dailyVisitsData;
            }
        }
    }

    public void Update(DateTime now)
    {
        var nowDate = now.Date;

        if(_dailyVisitsData == null)
        {
            LastVisit = nowDate;
            return;
        }

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

        VersionIncreased?.Invoke();
        Visited?.Invoke(this, VisitsInRow, reset);
        LastVisit = nowDate;
    }
}
