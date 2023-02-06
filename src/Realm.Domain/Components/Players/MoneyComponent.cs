namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class MoneyComponent : Component
{
    [Inject]
    private RealmConfigurationProvider RealmConfigurationProvider { get; set; } = default!;

    private decimal _money = 0;
    private readonly ReaderWriterLockSlim _moneyLock = new();

    public event Action<decimal>? MoneySet;
    public event Action<decimal>? MoneyAdded;
    public event Action<decimal>? MoneyTaken;
    public decimal Money
    {
        get => _money; set
        {
            ThrowIfDisposed();

            value = Normalize(value);

            if (Math.Abs(value) > RealmConfigurationProvider.GetRequired<decimal>("Gameplay:MoneyLimit"))
                throw new GameplayException("Unable to set money beyond limit.");

            _moneyLock.EnterWriteLock();
            try
            {
                if (_money == value)
                    return;
                _money = value;
                MoneySet?.Invoke(_money);
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
        var moneyPrecision = RealmConfigurationProvider.GetRequired<byte>("Gameplay:MoneyPrecision");
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
            if (Math.Abs(_money) + amount > RealmConfigurationProvider.GetRequired<decimal>("Gameplay:MoneyLimit"))
                throw new GameplayException("Unable to give money beyond limit.");

            _money += amount;
            MoneyAdded?.Invoke(amount);
        }
        catch(Exception)
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
            if (Math.Abs(_money) + amount > RealmConfigurationProvider.GetRequired<decimal>("Gameplay:MoneyLimit"))
                throw new GameplayException("Unable to take money beyond limit.");

            if(_money - amount < 0 && !force)
                throw new GameplayException("Unable to take money, not enough money.");

            _money -= amount;
            MoneyTaken?.Invoke(amount);
        }
        catch(Exception)
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
        if (amount == 0)
            return;

        TakeMoney(amount, force);
        moneyComponent.GiveMoney(amount);
    }
}
