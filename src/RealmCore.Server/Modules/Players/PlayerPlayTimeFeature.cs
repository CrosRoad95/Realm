﻿namespace RealmCore.Server.Modules.Players;

public interface IPlayerPlayTimeFeature : IPlayerFeature
{
    TimeSpan PlayTime { get; }
    TimeSpan TotalPlayTime { get; }

    event Action<IPlayerPlayTimeFeature>? MinutePlayed;
    event Action<IPlayerPlayTimeFeature>? MinuteTotalPlayed;

    void InternalSetTotalPlayTime(ulong time);
    void Reset();
}

internal sealed class PlayerPlayTimeFeature : IPlayerPlayTimeFeature
{
    private readonly object _lock = new();
    private DateTime _startDateTime;
    private ulong _totalPlayTime = 0;
    private readonly IPlayerUserFeature _playerUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUpdateService _updateService;
    private int _lastMinute = 0;
    private int _lastMinuteTotal = -1;

    public event Action<IPlayerPlayTimeFeature>? MinutePlayed;
    public event Action<IPlayerPlayTimeFeature>? MinuteTotalPlayed;

    public TimeSpan PlayTime => _dateTimeProvider.Now - _startDateTime;
    public TimeSpan TotalPlayTime => PlayTime + TimeSpan.FromSeconds(_totalPlayTime);

    public RealmPlayer Player { get; init; }
    public PlayerPlayTimeFeature(PlayerContext playerContext, IPlayerUserFeature playerUserService, IDateTimeProvider dateTimeProvider, IUpdateService updateService)
    {
        Player = playerContext.Player;
        _playerUserService = playerUserService;
        _dateTimeProvider = dateTimeProvider;
        _updateService = updateService;
        _startDateTime = _dateTimeProvider.Now;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _updateService.RareUpdate += HandleRareUpdate;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _totalPlayTime = playerUserService.User.PlayTime;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _totalPlayTime = 0;
    }

    public void InternalSetTotalPlayTime(ulong time)
    {
        _totalPlayTime = time;
    }

    public void Reset()
    {
        _startDateTime = _dateTimeProvider.Now;
    }

    public void HandleRareUpdate()
    {
        if ((int)PlayTime.TotalMinutes != _lastMinute)
        {
            _lastMinute = (int)PlayTime.TotalMinutes;
            MinutePlayed?.Invoke(this);
        }

        if ((int)TotalPlayTime.TotalMinutes != _lastMinuteTotal)
        {
            _lastMinuteTotal = (int)TotalPlayTime.TotalMinutes;
            MinuteTotalPlayed?.Invoke(this);
            _playerUserService.IncreaseVersion();
        }
    }
}