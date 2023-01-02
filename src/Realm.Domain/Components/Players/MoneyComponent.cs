namespace Realm.Domain.Components.Players;

public class MoneyComponent : Component
{
    [Inject]
    private RealmConfigurationProvider RealmConfigurationProvider { get; set; } = default!;

    private decimal _money = 0;

    public event Action<decimal>? MoneySet;
    public event Action<decimal>? MoneyAdded;
    public event Action<decimal>? MoneyRemoved;
    public decimal Money
    {
        get => _money; set
        {
            if (_money == value)
                return;

            if (value < 0)
                throw new GameplayException("Unable to set money, money can not get negative");

            if (value > RealmConfigurationProvider.GetRequired<decimal>("Gameplay:MoneyLimit"))
                throw new GameplayException("Unable to set money, reached limit.");
            _money = Normalize(value);
            MoneySet?.Invoke(_money);
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
        var moneyPrecision = RealmConfigurationProvider.GetRequired<int>("Gameplay:MoneyPrecision");
        return Math.Round(amount, moneyPrecision, MidpointRounding.AwayFromZero);
    }

    public bool GiveMoney(decimal amount)
    {

        if (_money < 0)
            throw new GameplayException("Unable to give money, money can not get negative");

        if (amount > RealmConfigurationProvider.GetRequired<decimal>("Gameplay:MoneyLimit"))
            throw new GameplayException("Unable to give money, reached limit.");

        _money += Normalize(amount);
        MoneyAdded?.Invoke(_money);
        return true;
    }

    public bool TakeMoney(decimal amount)
    {
        if (_money > 0)
            throw new GameplayException("Unable to give money, money can not get negative");

        var moneyPrecision = RealmConfigurationProvider.GetRequired<int>("Gameplay:MoneyPrecision");
        _money -= Normalize(amount);
        MoneyRemoved?.Invoke(_money);
        return true;
    }
}
