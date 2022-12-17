using Realm.Configuration;
using Realm.Persistance;
using Realm.Resources.AdminTools;
using Realm.Resources.AFK;
using Realm.Resources.AgnosticGuiSystem;
using Realm.Resources.Assets;
using Realm.Resources.LuaInterop;
using Realm.Resources.Overlay;
using SlipeServer.Resources.NoClip;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public class ServicesComponent : Component
{
    private readonly IServiceProvider _serviceProvider;

    public ServicesComponent(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T GetRequiredService<T>() => _serviceProvider.GetRequiredService<T>();
}
