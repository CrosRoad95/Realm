﻿namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class PlayTimeComponent : Component
{
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    private DateTime? _startDateTime;
    private readonly ulong _currentPlayTime = 0;

    public ulong PlayTime
    {
        get
        {
            ThrowIfDisposed();

            if (_startDateTime == null)
                return 0;
            return (ulong)(DateTimeProvider.Now - _startDateTime.Value).Seconds;
        }
    }

    public ulong TotalPlayTime
    {
        get
        {
            ThrowIfDisposed();
            return PlayTime + _currentPlayTime;
        }
    }

    public PlayTimeComponent()
    {
    }

    public PlayTimeComponent(ulong currentPlayTime)
    {
        _currentPlayTime = currentPlayTime;
    }

    protected override void Load()
    {
        _startDateTime = DateTimeProvider.Now;
    }

    public void Reset()
    {
        ThrowIfDisposed();

        _startDateTime = DateTimeProvider.Now;
    }
}