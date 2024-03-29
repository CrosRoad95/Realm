﻿namespace RealmCore.Server.Modules.Serving;

internal sealed class RealmResourceServer : IResourceServer
{
    public static int _resourceCounter = 0;
    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        _resourceCounter++;
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        _resourceCounter--;
    }

    public void Start() { }

    public void Stop() { }
}
