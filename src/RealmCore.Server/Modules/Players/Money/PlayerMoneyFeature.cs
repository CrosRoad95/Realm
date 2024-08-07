﻿namespace RealmCore.Server.Modules.Players.Money;

public sealed class PlayerMoneyFeature : IPlayerFeature, IUsesUserPersistentData
{
    private readonly ReaderWriterLockSlimScopedAsync _lock = new();
    private decimal _money = 0;
    private decimal _moneyLimit;
    private byte _moneyPrecision;
    private UserData? _userData;

    public RealmPlayer Player { get; init; }

    public event Action<PlayerMoneyFeature, decimal>? Set;
    public event Action<PlayerMoneyFeature, decimal>? Added;
    public event Action<PlayerMoneyFeature, decimal>? Taken;
    public event Action? VersionIncreased;

    public decimal Limit => _moneyLimit;
    public byte Precision => _moneyPrecision;

    public decimal Amount
    {
        get => _money;
        set
        {
            value = Normalize(value);

            if (Math.Abs(value) > _moneyLimit)
                throw new GameplayException($"Unable to set money beyond limit ({_moneyLimit}).");

            using var _ = _lock.Begin();

            if (_money == value)
                return;
            _money = value;
            SyncMoney();
            VersionIncreased?.Invoke();
            Set?.Invoke(this, _money);
        }
    }

    public PlayerMoneyFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        _userData = userData;
        _money = userData.Money;
    }

    public void SetInternal(decimal amount)
    {
        _money = amount;
        SyncMoney();
    }

    public void SetLimitAndPrecision(decimal moneyLimit, byte precision)
    {
        _moneyLimit = moneyLimit;
        _moneyPrecision = precision;
    }

    private decimal Normalize(decimal amount) => amount.Truncate(_moneyPrecision);

    private void SyncMoney()
    {
        if (_userData != null)
            _userData.Money = _money;
    }

    public void Give(decimal amount)
    {
        if (amount == 0)
            return;

        amount = Normalize(amount);

        if (amount < 0)
            throw new GameplayException("Unable to give money, amount can not get negative.");

        using var _ = _lock.Begin();

        if (Math.Abs(_money) + amount > _moneyLimit)
            throw new GameplayException("Unable to give money beyond limit.");

        _money += amount;
        SyncMoney();
        VersionIncreased?.Invoke();
        Added?.Invoke(this, amount);
    }

    private void TakeMoneyCore(decimal amount, bool force = false)
    {
        if (amount == 0)
            return;

        amount = Normalize(amount);

        if (amount < 0)
            throw new GameplayException("Unable to take money, amount can not get negative.");

        if (Math.Abs(_money) - amount < -_moneyLimit)
            throw new GameplayException("Unable to take money beyond limit.");

        if (_money - amount < 0 && !force)
            throw new GameplayException("Unable to take money, not enough money.");

        _money -= amount;
        SyncMoney();
        VersionIncreased?.Invoke();
        Taken?.Invoke(this, amount);
    }

    public void Take(decimal amount, bool force = false)
    {
        using var _ = _lock.Begin();
        TakeMoneyCore(amount, force);
    }

    internal bool HasMoneyCore(decimal amount, bool force = false) => _money >= amount || force;

    public bool Has(decimal amount, bool force = false)
    {
        using var _ = _lock.Begin();
        return HasMoneyCore(amount, force);
    }

    public bool TryTake(decimal amount, bool force = false)
    {
        try
        {
            Take(amount, force);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryTake(decimal amount, Func<bool> action, bool force = false)
    {
        using var _ = _lock.Begin();
        if (!HasMoneyCore(amount, force))
                return false;

        if (action())
        {
            TakeMoneyCore(amount, force);
            return true;
        }
        return false;
    }

    public async Task<bool> TryTakeAsync(decimal amount, Func<Task<bool>> action, bool force = false, CancellationToken cancellationToken = default)
    {
        using var _ = await _lock.BeginAsync(cancellationToken);
        if (!HasMoneyCore(amount, force))
            return false;

        if (await action())
        {
            TakeMoneyCore(amount, force);
            return true;
        }
        return false;
    }

    public void Transfer(PlayerMoneyFeature destination, decimal amount, bool force = false)
    {
        if (amount <= 0)
            throw new GameplayException("Unable to transfer money, amount can smaller or equal to zero.");

        Take(amount, force);
        destination.Give(amount);
    }
}
