﻿using SlipeServer.Packets.Definitions.Lua;

namespace RealmCore.Resources.Nametags;

public interface INametagsEventHub
{
    void AddNametags(LuaValue nametags);
    void SetPedNametag(LuaValue nametag);
    void RemoveNametag();
    void SetRenderingEnabled(bool enabled);
    void SetLocalPlayerRenderingEnabled(bool enabled);
}
