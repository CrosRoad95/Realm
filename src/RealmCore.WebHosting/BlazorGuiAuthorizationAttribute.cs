using Microsoft.AspNetCore.Authorization;

namespace RealmCore.WebHosting;

public class BlazorGuiAuthorizationAttribute : AuthorizeAttribute
{
    public BlazorGuiAuthorizationAttribute() : base("Cookies")
    {

    }
}
