namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class MoneyComponent : Component
{
    [Inject]
    private IOptions<GameplayOptions> GameplayOptions { get; set; } = default!;

    private decimal _money = 0;
    private readonly ReaderWriterLockSlim _moneyLock = new();

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

    private decimal Truncate(decimal d, byte decimals)
    {
        decimal r = Math.Round(d, decimals);

        if (d > 0 && r > d)
        {
            return r - new decimal(1, 0, 0, false, decimals);
        }
        else if (d < 0 && r < d)
        {
            return r + new decimal(1, 0, 0, false, decimals);
        }

        return r;
    }

    private decimal Normalize(decimal amount)
    {
        var moneyPrecision = GameplayOptions.Value.MoneyPrecision;
        return Truncate(amount, moneyPrecision);
    }

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

        _moneyLock.EnterWriteLock();
        try
        {
            if (Math.Abs(_money) + amount > GameplayOptions.Value.MoneyLimit)
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
