namespace RealmCore.BlazorCEFGui.Extensions;

internal static class NavigationManagerExtensions
{
    public static bool IsDev(this NavigationManager navigationManager)
    {
        return navigationManager.Uri.Contains("localhost");
    }
}
