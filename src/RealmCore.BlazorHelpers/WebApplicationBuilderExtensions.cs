namespace RealmCore.BlazorHelpers;

public static class WebApplicationBuilderExtensions
{
    public static void AddRealmBlazorGuiSupport(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("Cookies").AddCookie(x =>
        {
            x.LoginPath = "/realmGuiInitializeNotAllowed";
        });
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("Cookies", policy =>
            {
                policy.AddAuthenticationSchemes("Cookies");
                policy.RequireAuthenticatedUser();
            });
    }
}
