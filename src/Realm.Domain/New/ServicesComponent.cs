namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public class ServicesComponent : Component
{
    private readonly IServiceProvider _serviceProvider;

    public ServicesComponent(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
