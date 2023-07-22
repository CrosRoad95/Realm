namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class MoneyComponent : Component
{
    [Inject]
    private IOptions<GameplayOptions> GameplayOptions { get; set; } = default!;

    private decimal _money = 0;
    private readonly ReaderWriterLockSlim _moneyLock = new(LockRecursionPolicy.SupportsRecursion);
    private int writeLockRecursionCount = 0;

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

            if (Math.Abs(value) > GameplayOptions.Value.MoneyLimit)
                throw new GameplayException("Unable to set money beyond limit.");

            _moneyLock.EnterWriteLock();
            try
            {
                if (_money == value)
                    return;
                _money = value;
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

    public MoneyComponent()
    {
        _money = 0;
    }

    public MoneyComponent(decimal initialMoney)
    {
        _money = initialMoney;
    }

    private decimal Normalize(decimal amount)
    {
        var moneyPrecision = GameplayOptions.Value.MoneyPrecision;
        return amount.Truncate(moneyPrecision);
    }

    public void GiveMoney(decimal amount)
    {
        ThrowIfDisposed();

        if (amount == 0)
            return;

        amount = Normalize(amount);

        if (amount < 0)
            throw new GameplayException("Unable to give money, amount can not get negative.");

        if (++writeLockRecursionCount == 1)
            _moneyLock.EnterWriteLock();
        try
        {
            if (Math.Abs(_money) + amount > GameplayOptions.Value.MoneyLimit)
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
            if (--writeLockRecursionCount == 0)
                _moneyLock.ExitWriteLock();
        }
    }

    public void TakeMoney(decimal amount, bool force = false)
    {
        ThrowIfDisposed();

        if (amount == 0)
            return;

        amount = Normalize(amount);

        if (amount < 0)
            throw new GameplayException("Unable to take money, amount can not get negative.");

        if(++writeLockRecursionCount == 1)
            _moneyLock.EnterWriteLock();
        try
        {
            if (Math.Abs(_money) - amount < -GameplayOptions.Value.MoneyLimit)
                throw new GameplayException("Unable to take money beyond limit.");

            if (_money - amount < 0 && !force)
                throw new GameplayException("Unable to take money, not enough money.");

            _money -= amount;
            MoneyTaken?.Invoke(this, amount);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if(--writeLockRecursionCount == 0)
                _moneyLock.ExitWriteLock();
        }
    }

    internal bool InternalHasMoney(decimal amount, bool force = false) => (_money > amount) || force;

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

        if (++writeLockRecursionCount == 1)
            _moneyLock.EnterWriteLock();
        try
        {
            if(!InternalHasMoney(amount, force))
                return false;

            if(action())
            {
                TakeMoney(amount, force);
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
            if (--writeLockRecursionCount == 0)
                _moneyLock.ExitWriteLock();
        }
    }

    public async Task<bool> TryTakeMoneyWithCallbackAsync(decimal amount, Func<Task<bool>> action, bool force = false)
    {
        ThrowIfDisposed();

        if (++writeLockRecursionCount == 1)
            _moneyLock.EnterWriteLock();
        try
        {
            if(!InternalHasMoney(amount, force))
                return false;

            if(await action())
            {
                TakeMoney(amount, force);
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
            if (--writeLockRecursionCount == 0)
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
