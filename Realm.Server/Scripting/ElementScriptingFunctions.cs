﻿using Realm.Server.ElementCollections;
using Realm.Server.Elements.Variants;
using SlipeServer.Server.Elements.IdGeneration;

namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class ElementScriptingFunctions
{
    private readonly RPGServer _rpgServer;
    private readonly IElementCollection _elementCollection;
    private readonly IElementIdGenerator _elementIdGenerator;
    private readonly ElementByStringIdCollection _elementByStringIdCollection;

    public ElementScriptingFunctions(RPGServer rpgServer, IElementCollection elementCollection, IElementIdGenerator elementIdGenerator, ElementByStringIdCollection elementByStringIdCollection)
    {
        _rpgServer = rpgServer;
        _elementCollection = elementCollection;
        _elementIdGenerator = elementIdGenerator;
        _elementByStringIdCollection = elementByStringIdCollection;
    }

    [ScriptMember("setElementId")]
    public bool SetElementId(Element element, string id)
    {
        return _elementByStringIdCollection.AssignElementToId(element, id);
    }

    [ScriptMember("getElementId")]
    public string? GetElementId(Element element)
    {
        return _elementByStringIdCollection.GetElementId(element);
    }

    [ScriptMember("getElementById")]
    public Element? GetElementById(string id)
    {
        return _elementByStringIdCollection.GetElementById(id);
    }
    
    [ScriptMember("createSpawn")]
    public Spawn CreateSpawn(Vector3 position, Vector3? rotation = null)
    {
        var spawn = _rpgServer.GetRequiredService<Spawn>();
        spawn.Position = position;
        if (rotation != null)
            spawn.Rotation = rotation ?? Vector3.Zero;
        _rpgServer.AssociateElement(spawn);
        return spawn;
    }

    [ScriptMember("createVehicle")]
    public RPGVehicle CreateVehicle(ushort model, Vector3 position, Vector3? rotation = null)
    {
        var vehicle = _rpgServer.GetRequiredService<RPGVehicle>();
        vehicle.Model = model;
        vehicle.Position = position;
        if(rotation != null)
            vehicle.Rotation = rotation ?? Vector3.Zero;
        _rpgServer.AssociateElement(vehicle);
        return vehicle;
    }

    [ScriptMember("createBlip")]
    public RPGBlip CreateBlip(Vector3 position, int icon)
    {
        if (!Enum.IsDefined(typeof(BlipIcon), icon))
            throw new Exception("Invalid icon");

        var blip = _rpgServer.GetRequiredService<RPGBlip>();
        blip.Icon = (BlipIcon)icon;
        blip.Position = position;
        _rpgServer.AssociateElement(blip);
        return blip;
    }

    [ScriptMember("createVariantBlip")]
    public RPGVariantBlip CreateVariantBlip(Vector3 position)
    {
        var blip = _rpgServer.GetRequiredService<RPGBlip>();
        blip.Id = _elementIdGenerator.GetId();
        blip.SetIsVariant();
        blip.Position = position;
        return new RPGVariantBlip(blip);
    }

    [ScriptMember("createRadarArea")]
    public RPGRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color)
    {
        var radarArea = _rpgServer.GetRequiredService<RPGRadarArea>();
        radarArea.Position2 = position;
        radarArea.Size = size;
        radarArea.Color = color;
        _rpgServer.AssociateElement(radarArea);
        return radarArea;
    }

    [ScriptMember("createVariantRadarArea")]
    public RPGVariantRadarArea CreateVariantRadarArea(Vector2 position, Vector2 size)
    {
        var variant = _rpgServer.GetRequiredService<RPGRadarArea>();
        variant.Id = _elementIdGenerator.GetId();
        variant.SetIsVariant();
        variant.Position2 = position;
        variant.Size = size;
        return new RPGVariantRadarArea(variant);
    }
    

    [ScriptMember("createPickup")]
    public RPGPickup CreatePickup(Vector3 position, ushort model)
    {
        var pickup = _rpgServer.GetRequiredService<RPGPickup>();
        pickup.Position = position;
        pickup.Model = model;
        _rpgServer.AssociateElement(pickup);
        return pickup;
    }

    [ScriptMember("createVariantRadarArea")]
    public RPGVariantPickup CreateVariantRadarArea(Vector3 position, ushort model)
    {
        var variant = _rpgServer.GetRequiredService<RPGPickup>();
        variant.Id = _elementIdGenerator.GetId();
        variant.SetIsVariant();
        variant.Position = position;
        variant.Model = model;
        return new RPGVariantPickup(variant);
    }

    [ScriptMember("createFraction")]
    public RPGFraction CreateFraction(string code, string name, Vector3 position)
    {
        var fraction = _rpgServer.GetRequiredService<RPGFraction>();
        fraction.Code = code;
        fraction.Name = name;
        fraction.Position = position;
        return fraction;
    }

    public IEnumerable<object> GetCollectionByType(string type)
    {
        return type switch
        {
            "spawn" => _elementCollection.GetByType<Spawn>().Cast<object>(),
            "player" => _elementCollection.GetByType<Player>().Cast<object>(),
            "vehicle" => _elementCollection.GetByType<RPGVehicle>().Cast<object>(),
            "blip" => _elementCollection.GetByType<RPGBlip>().Cast<object>(),
            "radararea" => _elementCollection.GetByType<RPGRadarArea>().Cast<object>(),
            "pickup" => _elementCollection.GetByType<RPGPickup>().Cast<object>(),
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
        switch (element)
        {
            case Player _:
                throw new Exception("Can not destroy persistant element.");
            case Spawn spawn:
                if (spawn.IsPersistant())
                    throw new Exception("Can not destroy persistant element.");
                break;
            case RPGVehicle vehicle:
                if (vehicle.IsPersistant())
                    throw new Exception("Can not destroy persistant element.");
                break;
        }

        if (IsElement(element))
        {
            _elementCollection.Remove(element);
            if(element is IDisposable)
                ((IDisposable)element).Dispose();
            return true;
        }

        return false;
    }

    [ScriptMember("isElement")]
    public bool IsElement(Element element)
    {
        if (_elementCollection.Get(element.Id) != null)
            return true;
        return _elementCollection.GetAll().Any(x => x == element);
    }
}