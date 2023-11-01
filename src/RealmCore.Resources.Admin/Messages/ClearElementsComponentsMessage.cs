using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin.Messages;
internal record struct ClearElementsComponentsMessage(Player Player) : IMessage;

