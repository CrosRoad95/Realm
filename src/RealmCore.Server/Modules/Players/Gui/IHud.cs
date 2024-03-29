﻿namespace RealmCore.Server.Modules.Players.Gui;

public interface IHud : IDisposable
{
    Vector2 Offset { get; set; }
    void SetVisible(bool visible);
}

public interface IHud<TState> : IHud
{
    void UpdateState(Action<TState> callback);
}
