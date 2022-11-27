namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class GameplayScriptingFunctions
{
    private readonly RealmConfigurationProvider _configurationProvider;

    [ScriptMember("currency")]
    public string Currency { get => _configurationProvider.GetRequired<string>("Gameplay:Currency"); }

    [ScriptMember("moneyPrecision")]
    public int MoneyPrecision { get => _configurationProvider.GetRequired<int>("Gameplay:MoneyPrecision"); }
    [ScriptMember("moneyLimit")]
    public double MoneyLimit { get => _configurationProvider.GetRequired<double>("Gameplay:MoneyLimit"); }
    public GameplayScriptingFunctions(RealmConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }
}
