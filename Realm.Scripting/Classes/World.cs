﻿using Realm.Interfaces.Scripting.Classes;
using Realm.Interfaces.Server;

namespace Realm.Scripting.Classes;

public class World : IWorld
{
    private readonly ISpawnManager _spawnManager;

    public World(ISpawnManager spawnManager)
    {
        _spawnManager = spawnManager;
    }

    public ISpawn CreateSpawn(string name, Vector3 position, Vector3? rotation = null)
    {
        var id = _spawnManager.CreateSpawn(name, position, rotation ?? Vector3.Zero);
        return new Spawn(_spawnManager, id);
    }
}