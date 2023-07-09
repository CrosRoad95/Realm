namespace RealmCore.Server.Concepts.Interface;

public interface IMap
{
    List<Player> CreatedForPlayers { get; }
    BoundingBox BoundingBox { get; }

    bool IsCreatedFor(Entity entity);
    bool IsCreatedFor(Player player);
    bool LoadForPlayer(Entity entity);
    bool LoadForPlayer(Player player);
}
