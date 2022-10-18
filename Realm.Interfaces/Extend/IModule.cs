using Microsoft.Extensions.DependencyInjection;

namespace Realm.Interfaces.Extend;

public interface IModule
{
    string Name { get; }
    void Configure(IServiceCollection services);
    void Init(IServiceProvider serviceProvider);
}
