using System.Text;

namespace RealmCore.Resources.GuiSystem;

public class GuiSystemOptions
{
    internal readonly Dictionary<string, byte[]?> _providers = [];
    internal readonly Dictionary<string, byte[]> _guis = [];
    internal byte[]? _selectedGuiProvider = null;

    public GuiSystemOptions()
    {
        _providers.Add("cegui", null);

    }
    public GuiSystemOptions AddGuiProvider(string name, byte[] luaCode)
    {
        name = $"{name}.lua";
        if (_providers.ContainsKey(name))
            throw new ArgumentException(null, nameof(name));

        _providers.Add(name, luaCode);
        return this;
    }

    public GuiSystemOptions SetGuiProvider(string name)
    {
        if (!_providers.ContainsKey($"{name}.lua"))
            throw new ArgumentException(null, nameof(name));

        _selectedGuiProvider = Encoding.UTF8.GetBytes($"selectedGuiProvider = \"{name}\"");
        return this;
    }

    public GuiSystemOptions AddGui(string name, byte[] luaCode)
    {
        if (_guis.ContainsKey(name))
            throw new ArgumentException(null, nameof(name));

        _guis.Add(name, luaCode);
        return this;
    }
}
