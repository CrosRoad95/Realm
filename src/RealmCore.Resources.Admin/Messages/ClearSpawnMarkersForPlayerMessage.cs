using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;
internal record struct ClearSpawnMarkersForPlayerMessage(Player Player) : IMessage;
