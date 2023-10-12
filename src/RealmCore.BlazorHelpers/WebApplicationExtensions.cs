namespace RealmCore.BlazorHelpers;

public static class WebApplicationExtensions
{
    public static void UseRealmMiddleware(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            //var extension = Path.GetExtension(context.Request.Path);
            //var path = context.Request.Path;

            //var realmRPG = context.RequestServices.GetRequiredService<IRealmServer>();
            //string? cookieKey = null;
            //if (context.Request.Query.TryGetValue("key", out var value))
            //{
            //    context.Response.Cookies.Append("key", value.ToString());
            //    cookieKey = value.ToString();
            //}
            //if (cookieKey != null || context.Request.Cookies.TryGetValue("key", out cookieKey))
            //{
            //    if (realmRPG.GetRequiredService<IBrowserGuiService>().TryGetEntityByKey(cookieKey, out var guiPageComponent) && guiPageComponent != null)
            //    {
            //        context.Features.Set(new PlayerContextFeature(guiPageComponent, guiPageComponent.Entity, realmRPG));
            //        await next(context);
            //        return;
            //    }
            //}

            await next(context);
        });
    }
}

