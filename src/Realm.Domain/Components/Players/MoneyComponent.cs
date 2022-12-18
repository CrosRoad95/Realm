using Realm.Common.Exceptions;
using Realm.Configuration;

namespace Realm.Domain.Components.Players;

public class MoneyComponent : Component
{
    private double _money = 0;

    [ScriptMember("money")]
    public double Money
    {
        get => _money; set
        {
            if (_money == value)
                return;

            if (value < 0)
                throw new GameplayException("Unable to set money, money can not get negative");

            var configurationProvider = Entity.GetRequiredService<RealmConfigurationProvider>();

            if (value > configurationProvider.GetRequired<double>("Gameplay:MoneyLimit"))
                throw new GameplayException("Unable to set money, reached limit.");
            var moneyPrecision = configurationProvider.GetRequired<int>("Gameplay:MoneyPrecision");
            _money = Math.Round(value, moneyPrecision);
        }
    }

    public MoneyComponent(double initialMoney = 0.0)
    {
        _money = initialMoney;
    }

    [ScriptMember("giveMoney")]
    public bool GiveMoney(double amount)
    {
        var configurationProvider = Entity.GetRequiredService<RealmConfigurationProvider>();

        if (_money < 0)
            throw new GameplayException("Unable to set money, money can not get negative");

        if (amount > configurationProvider.GetRequired<double>("Gameplay:MoneyLimit"))
            throw new GameplayException("Unable to set money, reached limit.");

        var moneyPrecision = configurationProvider.GetRequired<int>("Gameplay:MoneyPrecision");
        _money = _money + Math.Round(amount, moneyPrecision);
        return true;
    }
}
