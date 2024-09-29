using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Overlay;

internal record struct CreateLine3dMessage(IEnumerable<Player> Target, int id, PositionContext from, PositionContext to, Color color, float width, LuaValue effect) : IMessage;
internal record struct RemoveLine3dMessage(IEnumerable<Player> Target, int[] lines) : IMessage;
internal record struct AddEffect3dMessage(IEnumerable<Player> Target, ParticleEffect effect, PositionContext position, Vector3 direction, Color color, bool randomizeColors, int count, float brightness, float size, bool randomSizes, float life) : IMessage;
