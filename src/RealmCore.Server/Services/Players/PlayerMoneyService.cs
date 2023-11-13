namespace RealmCore.Server.Services.Players;

internal sealed class PlayerMoneyService : IPlayerMoneyService
{
    private decimal _money = 0;
    private decimal _previousMoney = 0;
    private decimal _moneyLimit;
    private byte _moneyPrecision;
    private readonly ReaderWriterLockSlim _moneyLock = new();
    private readonly IOptionsMonitor<GameplayOptions>? _gameplayOptions;
    private readonly IPlayerUserService _playerUserService;

    public RealmPlayer Player { get; private set; }

    public event Action<IPlayerMoneyService, decimal>? Set;
    public event Action<IPlayerMoneyService, decimal>? Added;
    public event Action<IPlayerMoneyService, decimal>? Taken;
    public decimal Amount
    {
        get => _money;
        set
        {
            value = Normalize(value);

            if (Math.Abs(value) > _moneyLimit)
                throw new GameplayException("Unable to set money beyond limit.");

            _moneyLock.EnterWriteLock();
            try
            {
                if (_money == value)
                    return;
                _money = value;
                TryUpdateVersion();

                if (Set == null)
                    return;

                Set?.Invoke(this, _money);
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

    public PlayerMoneyService(PlayerContext playerContext, IOptionsMonitor<GameplayOptions> gameplayOptions, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        _gameplayOptions = gameplayOptions;
        _playerUserService = playerUserService;
        _moneyLimit = _gameplayOptions.CurrentValue.MoneyLimit;
        _moneyPrecision = _gameplayOptions.CurrentValue.MoneyPrecision;
        _gameplayOptions.OnChange(HandleGameplayOptionsChanged);
    }

    private void TryUpdateVersion()
    {
        if (Math.Abs(_money - _previousMoney) > 200)
        {
            _previousMoney = _money;
            _playerUserService.IncreaseVersion();
        }
    }

    public void SetMoneyInternal(decimal amount)
    {
        _money = amount;
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        _moneyLimit = gameplayOptions.MoneyLimit;
        _moneyPrecision = gameplayOptions.MoneyPrecision;
    }

    private decimal Normalize(decimal amount) => amount.Truncate(_moneyPrecision);

    public void GiveMoney(decimal amount)
    {
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
            TryUpdateVersion();
            Added?.Invoke(this, amount);
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
        TryUpdateVersion();
        Taken?.Invoke(this, amount);
    }

    public void TakeMoney(decimal amount, bool force = false)
    {
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

    public async Task<bool> TryTakeMoneyAsync(decimal amount, Func<Task<bool>> action, bool force = false)
    {
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

    public void TransferMoney(RealmPlayer player, decimal amount, bool force = false)
    {
        if (amount == 0)
            return;

        TakeMoney(amount, force);
        player.Money.GiveMoney(amount);
    }
}
