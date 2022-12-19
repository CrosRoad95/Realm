namespace Realm.Server.Modules;

public class IdentityModule : IModule
{
    public string Name => "Identity";

    public void Configure(IServiceCollection services)
    {
    }

    public void Init(IServiceProvider serviceProvider)
    {
    }

    public T GetInterface<T>() where T : class => throw new NotSupportedException();

    public void PostInit(IServiceProvider serviceProvider)
    {
    }

    public int GetPriority() => 100;
}
