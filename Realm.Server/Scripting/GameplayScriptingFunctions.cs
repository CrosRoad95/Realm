namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class GameplayScriptingFunctions
{
    private readonly ConfigurationProvider _configurationProvider;

    [ScriptMember("currency")]
    public string Currency { [ScriptUsage()] get => _configurationProvider.Get<string>("Gameplay:Currency"); }
    public GameplayScriptingFunctions(ConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }
}
