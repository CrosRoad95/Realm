namespace Realm.Server.Scripting;

public class LocalizationScriptingFunctions
{
    private readonly Func<string?> _basePathFactory;
    private readonly ConfigurationProvider _configurationProvider;
    private readonly LuaInteropService _luaInteropService;
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private string _defaultLanguage = "pl";
    public LocalizationScriptingFunctions(Func<string?> basePathFactory, ConfigurationProvider configurationProvider, LuaInteropService luaInteropService)
    {
        _basePathFactory = basePathFactory;
        _configurationProvider = configurationProvider;
        _luaInteropService = luaInteropService;
        _defaultLanguage = _configurationProvider.Get<string>("Gameplay:DefaultLanguage");
        _luaInteropService.ClientCultureInfoUpdate += HandleClientSendLocalizationCode;
        Reload().Wait();
    }

    private void HandleClientSendLocalizationCode(Player player, CultureInfo culture)
    {
        Dictionary<string, string> translations;
        if (_translations.ContainsKey(culture.TwoLetterISOLanguageName))
            translations = _translations[culture.TwoLetterISOLanguageName];
        else
            translations = _translations[_defaultLanguage];
        ((RPGPlayer)player).TriggerClientEvent("updateTranslation", translations);
    }

    public async Task Reload()
    {
        // TODO: improve exceptions, error handling
        _defaultLanguage = _configurationProvider.Get<string>("Gameplay:DefaultLanguage");
        _translations.Clear();
        var files = Directory.GetFiles(Path.Join(_basePathFactory(), "Localization"));
        foreach (var item in files)
        {
            var id = Path.GetFileNameWithoutExtension(item);
            var json = await File.ReadAllTextAsync(item);
            _translations[id] = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? throw new Exception($"Failed to load translation from {item} file.");
        }
        if(!_translations.ContainsKey(_defaultLanguage))
        {
            throw new Exception("Could not find a default language translation file!");
        }
    }

    public bool TranslationExists(string langId, string name)
    {
        if (_translations.ContainsKey(langId))
            return _translations[langId].ContainsKey(name);
        return false;
    }

    public object GetAvailiableLanguages()
    {
        return _translations.Keys.ToArray().ToScriptArray();
    }
    
    public object? GetLanguageTranslations(string langId)
    {
        if(_translations.ContainsKey(langId))
            return _translations[langId].Keys.ToArray().ToScriptArray();
        return null;
    }

    public string TryTranslate(string langId, string name, string @default)
    {
        Dictionary<string, string> translations;
        if (_translations.ContainsKey(langId))
            translations = _translations[langId];
        else
            translations = _translations[_defaultLanguage];

        if (translations.TryGetValue(name, out var translation))
            return translation;

        return @default;
    }

    public string Translate(string langId, string name)
    {
        Dictionary<string, string> translations;
        if (_translations.ContainsKey(langId))
            translations = _translations[langId];
        else
            translations = _translations[_defaultLanguage];

        if (translations.TryGetValue(name, out var translation))
            return translation;

        throw new Exception($"Failed to find translation '{name}' for language '{langId}'");
    }

    public string TranslateFor(RPGPlayer rpgPlayer, string name)
    {
        Dictionary<string, string> translations;
        if (_translations.ContainsKey(rpgPlayer.Language))
            translations = _translations[rpgPlayer.Language];
        else
            translations = _translations[_defaultLanguage];

        if (translations.TryGetValue(name, out var translation))
            return translation;

        throw new Exception($"Failed to find translation '{name}' for player language '{rpgPlayer.Language}'");
    }
}
