﻿using Microsoft.AspNetCore.Identity;
using Realm.Domain;
using Realm.Domain.Components.Elements;
using Realm.Persistance.Data;

namespace Realm.Tests.Helpers;

public class EntityHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TestingServer _testingServer;

    public EntityHelper(TestingServer testingServer)
    {
        _testingServer = testingServer;
        _serviceProvider = _testingServer.GetRequiredService<IServiceProvider>();
    }

    public Entity CreatePlayerEntity()
    {
        var entity = new Entity(_serviceProvider, Guid.NewGuid().ToString()[..8], Entity.PlayerTag);
        entity.AddComponent(new PlayerElementComponent(_testingServer.AddFakePlayer()));
        return entity;
    }
}