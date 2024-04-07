namespace RealmCore.Server.Modules.Players.Money;

public interface IPlayerMoneyFeature : IPlayerFeature
{
    decimal Amount { get; set; }
    decimal Limit { get; }
    byte Precision { get; }

    event Action<IPlayerMoneyFeature, decimal>? Set;
    event Action<IPlayerMoneyFeature, decimal>? Added;
    event Action<IPlayerMoneyFeature, decimal>? Taken;

    internal void SetInternal(decimal amount);
    void Give(decimal amount);
    bool Has(decimal amount, bool force = false);
    void Take(decimal amount, bool force = false);
    bool TryTake(decimal amount, bool force = false);
    bool TryTake(decimal amount, Func<bool> action, bool force = false);
    Task<bool> TryTakeAsync(decimal amount, Func<Task<bool>> action, bool force = false);
    void Transfer(IPlayerMoneyFeature destination, decimal amount, bool force = false);
    void SetLimitAndPrecision(decimal moneyLimit, byte precision);
}

internal sealed class PlayerMoneyFeature : IPlayerMoneyFeature, IUsesUserPersistentData, IDisposable
{
    private readonly ReaderWriterLockSlimScoped _lock = new();
    private decimal _money = 0;
    private decimal _moneyLimit;
    private byte _moneyPrecision;
    private UserData? _userData;

    public RealmPlayer Player { get; init; }

    public event Action<IPlayerMoneyFeature, decimal>? Set;
    public event Action<IPlayerMoneyFeature, decimal>? Added;
    public event Action<IPlayerMoneyFeature, decimal>? Taken;
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
                throw new GameplayException("Unable to set money beyond limit.");

            using var _ = _lock.BeginWrite();

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

    public void SignIn(UserData userData)
    {
        _userData = userData;
        _money = userData.Money;
    }

    public void SignOut()
    {
        Amount = 0;
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

        using var _ = _lock.BeginWrite();

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
        using var _ = _lock.BeginWrite();
        TakeMoneyCore(amount, force);
    }

    internal bool HasMoneyCore(decimal amount, bool force = false) => _money >= amount || force;

    public bool Has(decimal amount, bool force = false)
    {
        using var _ = _lock.BeginWrite();
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
        using var _ = _lock.BeginWrite();
        if (!HasMoneyCore(amount, force))
                return false;

        if (action())
        {
            TakeMoneyCore(amount, force);
            return true;
        }
        return false;
    }

    public async Task<bool> TryTakeAsync(decimal amount, Func<Task<bool>> action, bool force = false)
    {
        using var _ = _lock.BeginWrite();
        if (!HasMoneyCore(amount, force))
            return false;

        if (await action())
        {
            TakeMoneyCore(amount, force);
            return true;
        }
        return false;
    }

    public void Transfer(IPlayerMoneyFeature destination, decimal amount, bool force = false)
    {
        if (amount <= 0)
            throw new GameplayException("Unable to transfer money, amount can smaller or equal to zero.");

        Take(amount, force);
        destination.Give(amount);
    }

    public void Dispose()
    {
        Amount = 0;
    }
}
