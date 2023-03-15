namespace Realm.Server.Collections;

public sealed class EntityByStringIdCollection
{
    private readonly Dictionary<string, Entity> _entitiesById = new();
    private readonly Dictionary<Entity, string> _idByEntity = new();

    public bool AssignEntityToId(Entity entity, string id)
    {
        if (_entitiesById.ContainsKey(id))
            return false;
        _entitiesById[id] = entity;
        _idByEntity[entity] = id;
        entity.Disposed += HandleDestroyed;
        return true;
    }

    private void HandleDestroyed(Entity entity)
    {
        var id = GetEntityId(entity);
        if (id == null)
            throw new Exception("Failed to find entity id while destroying. A Bug?");

        _entitiesById.Remove(id);
        _idByEntity.Remove(entity);
        entity.Disposed -= HandleDestroyed;
    }

    public string? GetEntityId(Entity entity)
    {
        if (_idByEntity.TryGetValue(entity, out string? id))
            return id;
        return null;
    }

    public Entity? GetEntityById(string id)
    {
        if (_entitiesById.TryGetValue(id, out Entity? entity))
            return entity;
        return null;
    }
}
