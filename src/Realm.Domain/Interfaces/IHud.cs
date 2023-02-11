namespace Realm.Domain.Interfaces;

public interface IHud : IDisposable
{
    void SetVisible(bool visible);
}

public interface IHud<TState> : IHud
{
}
