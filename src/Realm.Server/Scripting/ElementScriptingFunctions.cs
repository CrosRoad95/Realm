using Realm.Domain.Elements.CollisionShapes;
using Realm.Domain.Elements.Variants;
using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class ElementScriptingFunctions
{
    private readonly IElementCollection _elementCollection;
    private readonly ElementByStringIdCollection _elementByStringIdCollection;
    private readonly VehicleUpgradeByStringCollection _vehicleUpgradeByStringCollection;
    private readonly IRPGElementsFactory _rpgElementsFactory;
    private readonly ILogger _logger;

    public ElementScriptingFunctions(IElementCollection elementCollection,
        ElementByStringIdCollection elementByStringIdCollection,
        VehicleUpgradeByStringCollection vehicleUpgradeByStringCollection,
        ILogger logger, IRPGElementsFactory rpgElementsFactory)
    {
        _elementCollection = elementCollection;
        _elementByStringIdCollection = elementByStringIdCollection;
        _vehicleUpgradeByStringCollection = vehicleUpgradeByStringCollection;
        _rpgElementsFactory = rpgElementsFactory;
        _logger = logger.ForContext<ElementScriptingFunctions>();
    }

    [ScriptMember("setElementId")]
    public bool SetElementId(Element element, string id)
        => _elementByStringIdCollection.AssignElementToId(element, id);

    [ScriptMember("getElementId")]
    public string? GetElementId(Element element)
        => _elementByStringIdCollection.GetElementId(element);

    [ScriptMember("getElementById")]
    public Element? GetElementById(string id)
        => _elementByStringIdCollection.GetElementById(id);

    [ScriptMember("getFractionById")]
    public RPGFraction? GetFractionById(string id)
        => _elementByStringIdCollection.GetElementById(id) as RPGFraction;

    [ScriptMember("createSpawn")]
    public RPGSpawn CreateSpawn(Vector3 position, Vector3? rotation = null, string? name = null)
        => _rpgElementsFactory.CreateSpawn(position, rotation, name);

    [ScriptMember("createVehicle")]
    public RPGVehicle CreateVehicle(ushort model, RPGSpawn spawn)
        => _rpgElementsFactory.CreateVehicle(model, spawn);    

    [ScriptMember("isVehicleIdAvailiable")]
    public async Task<bool> IsVehicleIdAvailiable(string id)
        => await _rpgElementsFactory.IsVehicleIdAvailiable(id);

    [ScriptMember("createNewPersistantVehicle")]
    public async Task<RPGVehicle?> CreateNewPersistantVehicle(string id, ushort model, RPGSpawn spawn)
        => await _rpgElementsFactory.CreateNewPersistantVehicle(id, model, spawn);

    [ScriptMember("spawnPersistantVehicle")]
    public async Task<RPGVehicle?> SpawnPersistantVehicle(string id, RPGSpawn spawn)
        => await _rpgElementsFactory.SpawnPersistantVehicle(id, spawn);

    [ScriptMember("createBlip")]
    public RPGBlip CreateBlip(Vector3 position, int icon)
        => _rpgElementsFactory.CreateBlip(position, icon);

    [ScriptMember("createVariantBlip")]
    public RPGVariantBlip CreateVariantBlip(Vector3 position)
        => _rpgElementsFactory.CreateVariantBlip(position);

    [ScriptMember("createRadarArea")]
    public RPGRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color)
        => _rpgElementsFactory.CreateRadarArea(position, size, color);

    [ScriptMember("createVariantRadarArea")]
    public RPGVariantRadarArea CreateVariantRadarArea(Vector2 position, Vector2 size)
        => _rpgElementsFactory.CreateVariantRadarArea(position, size);

    [ScriptMember("createPickup")]
    public RPGPickup CreatePickup(Vector3 position, ushort model)
        => _rpgElementsFactory.CreatePickup(position, model);

    [ScriptMember("createVariantRadarArea")]
    public RPGVariantPickup CreateVariantRadarArea(Vector3 position, ushort model)
        => _rpgElementsFactory.CreateVariantRadarArea(position, model);

    [ScriptMember("createFraction")]
    public RPGFraction CreateFraction(string code, string name, Vector3 position)
        => _rpgElementsFactory.CreateFraction(code, name, position);

    [ScriptMember("createColSphere")]
    public RPGCollisionSphere CreateColSphere(Vector3 position, float radius)
        => _rpgElementsFactory.CreateColSphere(position, radius);

    public IEnumerable<object> GetCollectionByType(string type)
    {
        return type switch
        {
            "spawn" => _elementCollection.GetAll().Where(x => x is RPGSpawn).Cast<object>(),
            "player" => _elementCollection.GetByType<Player>().Cast<object>(),
            "vehicle" => _elementCollection.GetByType<RPGVehicle>().Cast<object>(),
            "blip" => _elementCollection.GetByType<RPGBlip>().Cast<object>(),
            "radararea" => _elementCollection.GetByType<RPGRadarArea>().Cast<object>(),
            "pickup" => _elementCollection.GetByType<RPGPickup>().Cast<object>(),
            "fraction" => _elementCollection.GetAll().Where(x => x is RPGFraction).Cast<object>(),
            _ => throw new NotSupportedException($"Unsupported element type '{type}'")
        };
    }

    [ScriptMember("getElementsByType")]
    public object GetElementsByType(string type)
    {
        var elements = GetCollectionByType(type);
        return elements.ToArray().ToScriptArray();
    }

    [ScriptMember("countElementsByType")]
    public int CountElementsByType(string type)
    {
        var elements = GetCollectionByType(type);
        return elements.Count();
    }

    [ScriptMember("destroyElement")]
    public bool DestroyElement(Element element)
    {
        bool wasDestroyed = false;
        switch (element)
        {
            case Player _:
                throw new Exception("Can not destroy persistant element.");
            case RPGSpawn spawn:
                if (spawn.IsPersistant())
                    throw new Exception("Can not destroy persistant element.");
                wasDestroyed = true;
                break;
            case RPGVehicle vehicle:
                vehicle.Destroy();
                if(vehicle.IsPersistant())
                    _logger.Verbose("Destroyed persistant vehicle of id: {vehicleId}", vehicle.VehicleId);
                wasDestroyed = true;
                break;
        }

        if (wasDestroyed && IsElement(element))
        {
            _elementCollection.Remove(element);
            if(element is IDisposable)
                ((IDisposable)element).Dispose();
            return true;
        }

        return false;
    }

    [ScriptMember("getVehicleUpgradeByName")]
    public VehicleUpgrade? GetVehicleUpgradeByName(string name) => _vehicleUpgradeByStringCollection.GetElementById(name);

    [ScriptMember("isElement")]
    public bool IsElement(Element element)
    {
        if (_elementCollection.Get(element.Id) != null)
            return true;
        return _elementCollection.GetAll().Any(x => x == element);
    }
}
