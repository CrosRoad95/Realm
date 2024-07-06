namespace RealmCore.Server.Modules.World;

public class MapLoader
{
    private readonly IServiceProvider _serviceProvider;

    public MapLoader(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Map? LoadFromFile(string fileName, MapFormat mapFormat)
    {
        switch (mapFormat)
        {
            case MapFormat.Xml:
                {
                    XmlSerializer serializer = new(typeof(XmlMap), "");

                    XmlMap? xmlMap = null;
                    {
                        using var fileStream = File.OpenRead(fileName);
                        using var reader = new NamespaceIgnorantXmlTextReader(fileStream);
                        xmlMap = (XmlMap?)serializer.Deserialize(reader);
                    }

                    if (xmlMap == null ||
                        ((xmlMap.Objects == null || xmlMap.Objects.Length == 0) && (xmlMap.RemovedWorldModels == null || xmlMap.RemovedWorldModels.Length == 0)))
                        return null;

                    var objects = xmlMap.Objects != null ? xmlMap.Objects.Select(x => new WorldObject((ObjectModel)x.Model, new Vector3(x.PosX, x.PosY, x.PosZ))
                    {
                        Rotation = new Vector3(x.RotX, x.RotY, x.RotZ),
                        Interior = x.Interior,
                        Dimension = x.Dimension
                    }) : [];

                    var removeWorldModels = xmlMap.RemovedWorldModels != null ? xmlMap.RemovedWorldModels.Select(x =>
                        new RemoveWorldModel(x.Model, new Vector3(x.PosX, x.PosY, x.PosZ), x.Radius, x.Interior)) : [];

                    var map = ActivatorUtilities.CreateInstance<Map>(_serviceProvider, objects, removeWorldModels);
                    return map;
                }
        }
        return null;
    }
}
