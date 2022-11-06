namespace Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider;

public static class CeGuiGuiProvider
{
    public static string Name => "cegui";

    public static byte[] LuaCode { get; } = ResourceFiles.Provider;
}