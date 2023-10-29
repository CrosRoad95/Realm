namespace RealmCore.Sample.Logic;

internal class MapsLogic
{
    private readonly IMapsService _mapsService;
    private readonly IEntityEngine _entityEngine;

    public MapsLogic(IMapsService mapsService, IEntityEngine entityEngine)
    {
        _mapsService = mapsService;
        _entityEngine = entityEngine;
        _entityEngine.EntityCreated += HandleEntityCreated;

        //mapsService.RegisterMapFromMemory("testmap", new List<WorldObject>
        //{
        //    new WorldObject((ObjectModel)1337, new Vector3(-5, -5, -5)),
        //    new WorldObject((ObjectModel)1337, new Vector3(-10, -10, -10)),
        //    new WorldObject((ObjectModel)1337, new Vector3(-11, -10, -10)),
        //    new WorldObject((ObjectModel)1337, new Vector3(5, 5, 5)),
        //});

        //mapsService.RegisterMapsPath("C:\\Users\\sebaj\\source\\repos\\RealmCore\\src\\RealmCore.BlazorGui\\bin\\Debug\\net8.0\\Server\\Maps");
        mapsService.RegisterMapsPath("Server/Maps");
        //mapsService.RegisterMapFromXml("testmapxml", "Server/Maps/test.map");
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return;

        _mapsService.LoadAllMapsFor(entity);
    }
}
