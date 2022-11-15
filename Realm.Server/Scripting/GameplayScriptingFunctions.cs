namespace Realm.Server.Scripting;

public class GameplayScriptingFunctions
{
    private readonly ConfigurationProvider _configurationProvider;

    public string Currency => _configurationProvider.Get<string>("Gameplay:Currency");
    public GameplayScriptingFunctions(ConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }
}
