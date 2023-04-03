using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Realm.Extensions.RazorGui;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRazorGui(this IServiceCollection services)
    {
        services.AddRazorTemplating();
        services.AddSingleton<IRazorGui, RazorGui>();
        services.AddTransient<IGuiHelper, GuiHelper>();
        return services;
    }
}
