namespace Realm.Server.Scripting;

public class GameplayFunctions
{
    private readonly ConfigurationProvider _configurationProvider;

    public string Currency => _configurationProvider.Get<string>("Gameplay:Currency");
    public GameplayFunctions(ConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }
}
