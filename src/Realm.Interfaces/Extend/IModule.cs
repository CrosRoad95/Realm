using Microsoft.Extensions.DependencyInjection;

namespace Realm.Interfaces.Extend;

public interface IModule
{
    string Name { get; }
    public T GetInterface<T>() where T: class;
}
