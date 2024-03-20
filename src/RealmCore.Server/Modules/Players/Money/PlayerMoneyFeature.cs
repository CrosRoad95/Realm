namespace RealmCore.Server.Modules.Players.Money;

public interface IPlayerMoneyFeature : IPlayerFeature
{
    decimal Amount { get; set; }
    decimal Limit { get; }
    byte Precision { get; }

    event Action<IPlayerMoneyFeature, decimal>? Set;
    event Action<IPlayerMoneyFeature, decimal>? Added;
    event Action<IPlayerMoneyFeature, decimal>? Taken;

    internal void SetMoneyInternal(decimal amount);
    void GiveMoney(decimal amount);
    bool HasMoney(decimal amount, bool force = false);
    void TakeMoney(decimal amount, bool force = false);
    bool TryTakeMoney(decimal amount, bool force = false);
    bool TryTakeMoney(decimal amount, Func<bool> action, bool force = false);
    Task<bool> TryTakeMoneyAsync(decimal amount, Func<Task<bool>> action, bool force = false);
    void TransferMoney(RealmPlayer player, decimal amount, bool force = false);
    void SetMoneyLimitAndPrecision(decimal moneyLimit, byte precision);
}

internal sealed class PlayerMoneyFeature : IPlayerMoneyFeature, IUsesUserPersistentData, IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private decimal _money = 0;
    private decimal _previousMoney = 0;
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

            _lock.TryEnterWriteLock(TimeSpan.FromSeconds(15));
            try
            {
                if (_money == value)
                    return;
                _money = value;
                SyncMoney();

                TryUpdateVersion();

                Set?.Invoke(this, _money);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
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

    private void TryUpdateVersion()
    {
        if (Math.Abs(_money - _previousMoney) > 200)
        {
            _previousMoney = _money;
            VersionIncreased?.Invoke();
        }
    }

    public void SetMoneyInternal(decimal amount)
    {
        _money = amount;
        SyncMoney();
    }

    public void SetMoneyLimitAndPrecision(decimal moneyLimit, byte precision)
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

    public void GiveMoney(decimal amount)
    {
        if (amount == 0)
            return;

        amount = Normalize(amount);

        if (amount < 0)
            throw new GameplayException("Unable to give money, amount can not get negative.");

        _lock.TryEnterWriteLock(TimeSpan.FromSeconds(15));
        try
        {
            if (Math.Abs(_money) + amount > _moneyLimit)
                throw new GameplayException("Unable to give money beyond limit.");

            _money += amount;
            SyncMoney();
            TryUpdateVersion();
            Added?.Invoke(this, amount);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
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
        TryUpdateVersion();
        Taken?.Invoke(this, amount);
    }

    public void TakeMoney(decimal amount, bool force = false)
    {
        _lock.TryEnterWriteLock(TimeSpan.FromSeconds(15));
        try
        {
            TakeMoneyCore(amount, force);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal bool HasMoneyCore(decimal amount, bool force = false) => _money >= amount || force;

    public bool HasMoney(decimal amount, bool force = false)
    {
        _lock.TryEnterReadLock(TimeSpan.FromSeconds(15));
        try
        {
            return HasMoneyCore(amount, force);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool TryTakeMoney(decimal amount, bool force = false)
    {
        try
        {
            TakeMoney(amount, force);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryTakeMoney(decimal amount, Func<bool> action, bool force = false)
    {
        _lock.TryEnterWriteLock(TimeSpan.FromSeconds(15));
        try
        {
            if (!HasMoneyCore(amount, force))
                return false;

            if (action())
            {
                TakeMoneyCore(amount, force);
                return true;
            }
            return false;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<bool> TryTakeMoneyAsync(decimal amount, Func<Task<bool>> action, bool force = false)
    {
        _lock.TryEnterWriteLock(TimeSpan.FromSeconds(15));

        try
        {
            if (!HasMoneyCore(amount, force))
                return false;

            if (await action())
            {
                TakeMoneyCore(amount, force);
                return true;
            }
            return false;
        }
        catch
        {
            throw;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void TransferMoney(RealmPlayer player, decimal amount, bool force = false)
    {
        if (amount == 0)
            return;

        TakeMoney(amount, force);
        player.Money.GiveMoney(amount);
    }

    public void Dispose()
    {
        Amount = 0;
    }
}
