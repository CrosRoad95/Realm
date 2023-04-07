using System.Xml.Serialization;

namespace RealmCore.Server.Xml.Map;

[Serializable, XmlRoot(Namespace = "", ElementName = "map")]
public sealed class XmlMap
{
    [XmlElement("object")]
    public List<XmlObject> Objects { get; set; }
}
