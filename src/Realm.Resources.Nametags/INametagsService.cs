﻿using SlipeServer.Server.Elements;

namespace Realm.Resources.Nametags;

public interface INametagsService
{
    internal Action<Ped, string>? HandleSetNametag { get; set; }
    internal Action<Ped>? HandleRemoveNametag { get; set; }
    internal Action<Player, bool>? HandleSetNametagRenderingEnabled { get; set; }

    void RemoveNametag(Ped ped);
    void SetNametag(Ped ped, string text);
    void SetNametagRenderingEnabled(Player player, bool enabled);
}