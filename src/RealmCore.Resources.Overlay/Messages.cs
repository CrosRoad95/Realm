using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Overlay;

internal record struct CreateLine3dMessage(IEnumerable<Player> Target, int id, PositionContext from, PositionContext to, Color color, float width, LuaValue effect) : IMessage;
internal record struct RemoveLine3dMessage(IEnumerable<Player> Target, int[] lines) : IMessage;
