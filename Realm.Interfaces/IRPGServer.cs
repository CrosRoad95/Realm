﻿namespace Realm.Interfaces;

public interface IRPGServer
{
    event Action<IRPGPlayer>? PlayerJoined;

    TService GetRequiredService<TService>() where TService : notnull;
    void SubscribeLuaEvent(string eventName, Action<ILuaEventContext> callback);
}