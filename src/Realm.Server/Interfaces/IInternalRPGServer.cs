﻿namespace Realm.Server.Interfaces;

public interface IInternalRPGServer
{
    string MapName { get; set; }
    string GameType { get; set; }

    event Action<Entity>? PlayerJoined;

    Entity CreateEntity(string name);
    Entity GetEntityByPlayer(Player player);
    TService GetRequiredService<TService>() where TService : notnull;
}