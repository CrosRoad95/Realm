using Microsoft.AspNetCore.Components;

namespace RealmCore.BlazorLibrary.Extensions;

public static class NavigationManagerExtensions
{
    public static bool IsDev(this NavigationManager navigationManager)
    {
        return navigationManager.Uri.Contains("localhost");
    }
}
