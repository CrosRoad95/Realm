namespace RealmCore.Resources.Addons.GuiSystem.DGSProvider;

public static class DGSGuiProvider
{
    public static string Name => "dgs";

    public static byte[] LuaCode { get; } = ResourceFiles.Provider;
}