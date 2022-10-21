using Microsoft.Extensions.DependencyInjection;

namespace Realm.Interfaces.Extend;

public interface IModule : IReloadable
{
    string Name { get; }
    void Configure(IServiceCollection services);
    void Init(IServiceProvider serviceProvider);
    void PostInit(IServiceProvider serviceProvider);

    public T GetInterface<T>() where T: class;
}
