using Realm.Domain.Enums;
using Realm.Server;
using SlipeServer.Server.Enums;

namespace Realm.Console.Logic;

internal class MapsLogic
{
    private readonly IMapsService _mapsService;
    private readonly ECS _ecs;

    public MapsLogic(IMapsService mapsService, ECS ecs)
    {
        _mapsService = mapsService;
        _ecs = ecs;
        _ecs.EntityCreated += HandleEntityCreated;

        mapsService.RegisterMapFromMemory("testmap", new List<WorldObject>
        {
            new WorldObject((ObjectModel)1337, new Vector3(-5, -5, -5)),
            new WorldObject((ObjectModel)1337, new Vector3(-10, -10, -10)),
            new WorldObject((ObjectModel)1337, new Vector3(-11, -10, -10)),
            new WorldObject((ObjectModel)1337, new Vector3(5, 5, 5)),
        });

        mapsService.RegisterMapFromXml("testmapxml", "Server/Maps/test.map");
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != EntityTag.Player)
            return;

        _mapsService.LoadMapFor("testmap", entity);
        _mapsService.LoadMapFor("testmapxml", entity);
    }
}
