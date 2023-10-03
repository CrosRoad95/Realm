namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class MoneyComponent : Component
{
    private decimal _money = 0;
    private decimal _moneyLimit;
    private byte _moneyPrecision;
    private readonly ReaderWriterLockSlim _moneyLock = new();
    private readonly IOptionsMonitor<GameplayOptions>? _gameplayOptions;

    public event Action<MoneyComponent, decimal>? MoneyLimitChanged;
    public event Action<MoneyComponent, byte>? MoneyPrecisionChanged;
    public event Action<MoneyComponent, decimal>? MoneySet;
    public event Action<MoneyComponent, decimal>? MoneyAdded;
    public event Action<MoneyComponent, decimal>? MoneyTaken;
    public decimal Money
    {
        get
        {
            ThrowIfDisposed();
            return _money;
        }
        set
        {
            ThrowIfDisposed();

            value = Normalize(value);

            if (Math.Abs(value) > _moneyLimit)
                throw new GameplayException("Unable to set money beyond limit.");

            _moneyLock.EnterWriteLock();
            try
            {
                if (_money == value)
                    return;
                _money = value;

                if (MoneySet == null)
                    return;

                MoneySet?.Invoke(this, _money);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _moneyLock.ExitWriteLock();
            }
        }
    }

    public MoneyComponent(decimal initialMoney, IOptionsMonitor<GameplayOptions> gameplayOptions)
    {
        _money = initialMoney;
        _gameplayOptions = gameplayOptions;
        _moneyLimit = _gameplayOptions.CurrentValue.MoneyLimit;
        _moneyPrecision = _gameplayOptions.CurrentValue.MoneyPrecision;
        _gameplayOptions.OnChange(HandleGameplayOptionsChanged);
    }

    public MoneyComponent(decimal moneyLimit, byte moneyPrecision)
    {
        _money = 0;
        _moneyLimit = moneyLimit;
        _moneyPrecision = moneyPrecision;
    }

    public MoneyComponent(decimal initialMoney, decimal moneyLimit, byte moneyPrecision)
    {
        _money = initialMoney;
        _moneyLimit = moneyLimit;
        _moneyPrecision = moneyPrecision;
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        if (_moneyLimit != gameplayOptions.MoneyLimit)
        {
            _moneyLimit = gameplayOptions.MoneyLimit;
            MoneyLimitChanged?.Invoke(this, _moneyLimit);
        }
        if (_moneyPrecision != gameplayOptions.MoneyPrecision)
        {
            _moneyPrecision = gameplayOptions.MoneyPrecision;
            MoneyPrecisionChanged?.Invoke(this, _moneyPrecision);
        }
    }

    private decimal Normalize(decimal amount) => amount.Truncate(_moneyPrecision);

    public void GiveMoney(decimal amount)
    {
        ThrowIfDisposed();

        if (amount == 0)
            return;

        amount = Normalize(amount);

        if (amount < 0)
            throw new GameplayException("Unable to give money, amount can not get negative.");

        _moneyLock.EnterWriteLock();
        try
        {
            if (Math.Abs(_money) + amount > _moneyLimit)
                throw new GameplayException("Unable to give money beyond limit.");

            _money += amount;
            MoneyAdded?.Invoke(this, amount);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _moneyLock.ExitWriteLock();
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
        MoneyTaken?.Invoke(this, amount);
    }

    public void TakeMoney(decimal amount, bool force = false)
    {
        ThrowIfDisposed();

        _moneyLock.EnterWriteLock();
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
            _moneyLock.ExitWriteLock();
        }
    }

    internal bool InternalHasMoney(decimal amount, bool force = false) => (_money >= amount) || force;

    public bool HasMoney(decimal amount, bool force = false)
    {
        ThrowIfDisposed();

        _moneyLock.EnterReadLock();
        try
        {
            return InternalHasMoney(amount, force);
        }
        finally
        {
            _moneyLock.ExitReadLock();
        }
    }

    public bool TryTakeMoney(decimal amount, bool force = false)
    {
        ThrowIfDisposed();

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

    public bool TryTakeMoneyWithCallback(decimal amount, Func<bool> action, bool force = false)
    {
        ThrowIfDisposed();

        _moneyLock.EnterWriteLock();
        try
        {
            if (!InternalHasMoney(amount, force))
                return false;

            if (action())
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
            _moneyLock.ExitWriteLock();
        }
    }

    public async Task<bool> TryTakeMoneyWithCallbackAsync(decimal amount, Func<Task<bool>> action, bool force = false)
    {
        ThrowIfDisposed();

        _moneyLock.EnterWriteLock();

        try
        {
            if (!InternalHasMoney(amount, force))
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
            _moneyLock.ExitWriteLock();
        }
    }

    public void TransferMoney(MoneyComponent moneyComponent, decimal amount, bool force = false)
    {
        ThrowIfDisposed();

        if (amount == 0)
            return;

        TakeMoney(amount, force);
        moneyComponent.GiveMoney(amount);
    }
}
