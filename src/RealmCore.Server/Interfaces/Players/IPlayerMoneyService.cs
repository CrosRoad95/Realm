namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerMoneyService : IPlayerService
{
    decimal Amount { get; set; }

    event Action<IPlayerMoneyService, decimal>? MoneyLimitChanged;
    event Action<IPlayerMoneyService, byte>? MoneyPrecisionChanged;
    event Action<IPlayerMoneyService, decimal>? MoneySet;
    event Action<IPlayerMoneyService, decimal>? MoneyAdded;
    event Action<IPlayerMoneyService, decimal>? MoneyTaken;

    internal void SetMoneyInternal(decimal amount);
    void GiveMoney(decimal amount);
    bool HasMoney(decimal amount, bool force = false);
    void TakeMoney(decimal amount, bool force = false);
    bool TryTakeMoney(decimal amount, bool force = false);
    bool TryTakeMoneyWithCallback(decimal amount, Func<bool> action, bool force = false);
    Task<bool> TryTakeMoneyWithCallbackAsync(decimal amount, Func<Task<bool>> action, bool force = false);
    void TransferMoney(RealmPlayer player, decimal amount, bool force = false);
}
