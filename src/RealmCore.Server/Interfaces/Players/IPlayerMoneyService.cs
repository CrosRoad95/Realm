namespace RealmCore.Server.Interfaces.Players;

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
