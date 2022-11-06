namespace Realm.Resources.AgnosticGuiSystem;

public class AgnosticGuiSystemOptions
{
    internal readonly Dictionary<string, byte[]> _providers = new();
    internal readonly Dictionary<string, byte[]> _guis = new();

    public AgnosticGuiSystemOptions AddGuiProvider(string name, byte[] luaCode)
    {
        if (_guis.ContainsKey(name))
            throw new ArgumentException(null, nameof(name));

        return this;
    }

    public AgnosticGuiSystemOptions AddGui(string name, byte[] luaCode)
    {
        if (_guis.ContainsKey(name))
            throw new ArgumentException(null, nameof(name));

        return this;
    }
}
