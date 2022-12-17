using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class ElementScriptingFunctions
{
    private readonly IElementCollection _elementCollection;
    private readonly EntityByStringIdCollection _entityByStringIdCollection;
    private readonly VehicleUpgradeByStringCollection _vehicleUpgradeByStringCollection;
    private readonly EntityElementFactory _entityElementFactory;

    public ElementScriptingFunctions(IElementCollection elementCollection,
        EntityByStringIdCollection entityByStringIdCollection,
        VehicleUpgradeByStringCollection vehicleUpgradeByStringCollection,
        EntityElementFactory entityElementFactory)
    {
        _elementCollection = elementCollection;
        _entityByStringIdCollection = entityByStringIdCollection;
        _vehicleUpgradeByStringCollection = vehicleUpgradeByStringCollection;
        _entityElementFactory = entityElementFactory;
    }

    [ScriptMember("getEntityId")]
    public string? GetElementId(Entity entity)
        => _entityByStringIdCollection.GetEntityId(entity);

    [ScriptMember("getEntityById")]
    public Entity? GetElementById(string id)
        => _entityByStringIdCollection.GetEntityById(id);

    [ScriptMember("getVehicleUpgradeByName")]
    public VehicleUpgrade? GetVehicleUpgradeByName(string name) => _vehicleUpgradeByStringCollection.GetElementById(name);

    [ScriptMember("isElement")]
    public bool IsElement(Element element) => _elementCollection.IsElement(element);

    [ScriptMember("createVehicle")]
    public Entity CreateVehicle(ushort model, Vector3 position, Vector3? rotation = null)
    {
        return _entityElementFactory.CreateVehicle(model, position, rotation ?? Vector3.Zero);
    }
}
