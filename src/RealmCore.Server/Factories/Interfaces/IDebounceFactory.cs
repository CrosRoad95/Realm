
namespace RealmCore.Server.Factories.Interfaces;

public interface IDebounceFactory
{
    IDebounce Create(int milliseconds);
}
