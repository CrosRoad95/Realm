﻿namespace Realm.Resources.Assets.Interfaces;

public interface IServerAssetsProvider
{
    public IEnumerable<(string, byte[])> Provide();
}