namespace RealmCore.Sample.Logic;

internal class MapsLogic
{
    private readonly IMapsService _mapsService;
    private readonly MapsCollection _mapsCollection;
    private readonly ILogger<MapsLogic> _logger;

    public MapsLogic(IMapsService mapsService, MapsCollection mapsCollection, ILogger<MapsLogic> logger)
    {
        _mapsService = mapsService;
        _mapsCollection = mapsCollection;
        _logger = logger;
        _mapsCollection.MapChanged += HandleMapChanged;

        //mapsService.RegisterMapFromMemory("testmap", new List<WorldObject>
        //{
        //    new WorldObject((ObjectModel)1337, new Vector3(-5, -5, -5)),
        //    new WorldObject((ObjectModel)1337, new Vector3(-10, -10, -10)),
        //    new WorldObject((ObjectModel)1337, new Vector3(-11, -10, -10)),
        //    new WorldObject((ObjectModel)1337, new Vector3(5, 5, 5)),
        //});

        //mapsService.RegisterMapsPath("C:\\Users\\sebaj\\source\\repos\\RealmCore\\src\\RealmCore.BlazorGui\\bin\\Debug\\net8.0\\Server\\Maps");
        mapsCollection.RegisterMapsPath("Server/Maps", mapsService);
        //mapsService.RegisterMapFromXml("testmapxml", "Server/Maps/test.map");
    }

    private void HandleMapChanged(string mapName, MapEventType mapEventType)
    {
        _logger.LogInformation("Map {mapName} {mapEventType}", mapName, mapEventType);
        if (mapEventType == MapEventType.Add)
            _mapsService.Load(mapName);
    }
}
