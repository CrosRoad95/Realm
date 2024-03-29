﻿using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;

namespace RealmCore.Resources.Overlay;

internal class OverlayResource : Resource
{
    internal Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["utility.lua"] = ResourceFiles.Utility,
        ["hud.lua"] = ResourceFiles.Hud,
        ["notifications.lua"] = ResourceFiles.Notifications,
        ["displays.lua"] = ResourceFiles.Displays,
    };

    internal OverlayResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Overlay")
    {
        foreach (var (path, content) in AdditionalFiles)
            Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
