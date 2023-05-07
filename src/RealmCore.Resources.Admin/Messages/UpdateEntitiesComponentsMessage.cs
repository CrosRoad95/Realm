using RealmCore.Resources.Admin.Enums;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;

internal record struct UpdateEntitiesComponentsMessage(Player Player, LuaValue entitiesComponents) : IMessage;
