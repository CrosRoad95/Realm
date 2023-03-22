using System.Text;

namespace Realm.Resources.AgnosticGuiSystem;

public class AgnosticGuiSystemOptions
{
    internal readonly Dictionary<string, byte[]> _providers = new();
    internal readonly Dictionary<string, byte[]> _guis = new();
    internal byte[]? _selectedGuiProvider = null;

    public AgnosticGuiSystemOptions AddGuiProvider(string name, byte[] luaCode)
    {
        name = $"{name}.lua";
        if (_providers.ContainsKey(name))
            throw new ArgumentException(null, nameof(name));

        _providers.Add(name, luaCode);
        return this;
    }

    public AgnosticGuiSystemOptions SetGuiProvider(string name)
    {
        if (!_providers.ContainsKey($"{name}.lua"))
            throw new ArgumentException(null, nameof(name));

        _selectedGuiProvider = UTF8Encoding.UTF8.GetBytes($"selectedGuiProvider = \"{name}\"");
        return this;
    }

    public AgnosticGuiSystemOptions AddGui(string name, byte[] luaCode)
    {
        if (_guis.ContainsKey(name))
            throw new ArgumentException(null, nameof(name));

        _guis.Add(name, luaCode);
        return this;
    }
}
