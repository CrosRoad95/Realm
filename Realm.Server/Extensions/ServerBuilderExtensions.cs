namespace Realm.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddGuiFilesLocation(this ServerBuilder serverBuilder, string path = "Gui")
    {
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IGuiFilesProvider>(new GuiFilesProvider(path));
        });
        return serverBuilder;
    }
}
