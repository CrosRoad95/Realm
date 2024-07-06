namespace RealmCore.Server.Modules.World.Xml.Map;

public sealed class XmlObject
{
    [XmlAttribute("id")]
    public string Id { get; set; }
    [XmlAttribute("posX")]
    public float PosX { get; set; }
    [XmlAttribute("posY")]
    public float PosY { get; set; }
    [XmlAttribute("posZ")]
    public float PosZ { get; set; }
    [XmlAttribute("rotX")]
    public float RotX { get; set; }
    [XmlAttribute("rotY")]
    public float RotY { get; set; }
    [XmlAttribute("rotZ")]
    public float RotZ { get; set; }
    [XmlAttribute("interior")]
    public byte Interior { get; set; }
    [XmlAttribute("dimension")]
    public ushort Dimension { get; set; }
    [XmlAttribute("model")]
    public ushort Model { get; set; }
}

public sealed class XmlRemoveWorldModel
{
    [XmlAttribute("id")]
    public string Id { get; set; }
    [XmlAttribute("posX")]
    public float PosX { get; set; }
    [XmlAttribute("posY")]
    public float PosY { get; set; }
    [XmlAttribute("posZ")]
    public float PosZ { get; set; }
    [XmlAttribute("interior")]
    public byte Interior { get; set; }
    [XmlAttribute("radius")]
    public float Radius { get; set; }
    [XmlAttribute("model")]
    public ushort Model { get; set; }
}

[Serializable, XmlRoot(Namespace = "", ElementName = "map")]
public sealed class XmlMap
{
    [XmlElement("object")]
    public XmlObject[]? Objects { get; set; }
    [XmlElement("removeWorldObject")]
    public XmlRemoveWorldModel[]? RemovedWorldModels { get; set; }
}
