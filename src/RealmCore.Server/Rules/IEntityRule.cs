﻿namespace RealmCore.Server.Rules;

public interface IEntityRule
{
    bool Check(Entity entity);
}