﻿namespace Realm.Domain.Rules;

public sealed class MustBePlayerOnFootOnlyRule : IEntityRule
{
    public bool Check(Entity entity)
    {
        if (entity.Tag != Entity.PlayerTag)
            return false;

        return entity.GetRequiredComponent<PlayerElementComponent>().OccupiedVehicle == null;
    }
}