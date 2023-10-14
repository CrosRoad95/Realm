namespace RealmCore.BlazorHelpers;

public static class WebApplicationBuilderExtensions
{
    public static void AddRealmBlazorGuiSupport(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CurrentPlayerContext>();
        builder.Services.AddScoped(typeof(CurrentPlayerContext<>));

        builder.Services.AddAuthentication("Cookies").AddCookie(x =>
        {
            x.LoginPath = "/realmGuiInitialize";
        });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Cookies", policy =>
            {
                policy.AddAuthenticationSchemes("Cookies");
                policy.RequireAuthenticatedUser();
            });
        });

    }
}
