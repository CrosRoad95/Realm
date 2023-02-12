namespace Realm.Domain.Interfaces;

public interface IHud : IDisposable
{
    Vector2 Position { get; set; }
    void SetVisible(bool visible);
}

public interface IHud<TState> : IHud
{
    void UpdateState(Action<TState> callback);
}
