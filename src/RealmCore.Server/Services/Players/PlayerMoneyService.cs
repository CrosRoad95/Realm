namespace RealmCore.Server.Services.Players;

public interface IPlayerMoneyService : IPlayerService
{
    decimal Amount { get; set; }

    event Action<IPlayerMoneyService, decimal>? Set;
    event Action<IPlayerMoneyService, decimal>? Added;
    event Action<IPlayerMoneyService, decimal>? Taken;

    internal void SetMoneyInternal(decimal amount);
    void GiveMoney(decimal amount);
    bool HasMoney(decimal amount, bool force = false);
    void TakeMoney(decimal amount, bool force = false);
    bool TryTakeMoney(decimal amount, bool force = false);
    bool TryTakeMoney(decimal amount, Func<bool> action, bool force = false);
    Task<bool> TryTakeMoneyAsync(decimal amount, Func<Task<bool>> action, bool force = false);
    void TransferMoney(RealmPlayer player, decimal amount, bool force = false);
}

internal sealed class PlayerMoneyService : IPlayerMoneyService
{
    private readonly ReaderWriterLockSlim _lock = new();
    private decimal _money = 0;
    private decimal _previousMoney = 0;
    private decimal _moneyLimit;
    private byte _moneyPrecision;
    private readonly IOptionsMonitor<GameplayOptions>? _gameplayOptions;
    private readonly IPlayerUserService _playerUserService;

    public RealmPlayer Player { get; init; }

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

            _lock.EnterWriteLock();
            try
            {
                if (_money == value)
                    return;
                _money = value;

                if(_playerUserService.IsSignedIn)
                    _playerUserService.User.Money = _money;
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

    public PlayerMoneyService(PlayerContext playerContext, IOptionsMonitor<GameplayOptions> gameplayOptions, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        _gameplayOptions = gameplayOptions;
        _playerUserService = playerUserService;
        _moneyLimit = _gameplayOptions.CurrentValue.MoneyLimit;
        _moneyPrecision = _gameplayOptions.CurrentValue.MoneyPrecision;
        _gameplayOptions.OnChange(HandleGameplayOptionsChanged);

        _playerUserService.SignedIn += HandleSignedIn;
        _playerUserService.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserService userService, RealmPlayer player)
    {
        Amount = userService.User.Money;
    }

    private void HandleSignedOut(IPlayerUserService userService, RealmPlayer player)
    {
        Amount = 0;
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

        _lock.EnterWriteLock();
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
        TryUpdateVersion();
        Taken?.Invoke(this, amount);
    }

    public void TakeMoney(decimal amount, bool force = false)
    {
        _lock.EnterWriteLock();
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

    internal bool InternalHasMoney(decimal amount, bool force = false) => (_money >= amount) || force;

    public bool HasMoney(decimal amount, bool force = false)
    {
        _lock.EnterReadLock();
        try
        {
            return InternalHasMoney(amount, force);
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
        _lock.EnterWriteLock();
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
            _lock.ExitWriteLock();
        }
    }

    public async Task<bool> TryTakeMoneyAsync(decimal amount, Func<Task<bool>> action, bool force = false)
    {
        _lock.EnterWriteLock();

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
}
