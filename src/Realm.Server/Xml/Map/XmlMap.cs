using System.Xml.Serialization;

namespace Realm.Server.Xml.Map;

[Serializable, XmlRoot(Namespace = "", ElementName = "map")]
public sealed class XmlMap
{
    [XmlElement("object")]
    public List<XmlObject> Objects { get; set; }
}
