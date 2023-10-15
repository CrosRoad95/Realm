using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RealmCore.BlazorHelpers;

public static class WebApplicationBuilderExtensions
{
    public static void AddRealmBlazorGuiSupport(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CurrentPlayerContext>();
        builder.Services.AddScoped(typeof(CurrentPlayerContext<>));

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
