using System.Xml.Serialization;

namespace Realm.Server.Xml.Map;

public sealed class XmlObject
{
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
    public short Model { get; set; }
}
