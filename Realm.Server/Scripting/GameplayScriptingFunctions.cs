namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class GameplayScriptingFunctions
{
    private readonly ConfigurationProvider _configurationProvider;

    [ScriptMember("currency")]
    public string Currency { get => _configurationProvider.Get<string>("Gameplay:Currency"); }

    [ScriptMember("moneyPrecision")]
    public int MoneyPrecision { get => _configurationProvider.Get<int>("Gameplay:MoneyPrecision"); }
    [ScriptMember("moneyLimit")]
    public double MoneyLimit { get => _configurationProvider.Get<double>("Gameplay:MoneyLimit"); }
    public GameplayScriptingFunctions(ConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }
}
