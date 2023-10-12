using Microsoft.AspNetCore.Authorization;

namespace RealmCore.BlazorHelpers;

public class BlazorGuiAuthorizationAttribute : AuthorizeAttribute
{
    public BlazorGuiAuthorizationAttribute() : base("Cookies")
    {

    }
}
