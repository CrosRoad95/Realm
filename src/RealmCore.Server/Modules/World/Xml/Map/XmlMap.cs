namespace RealmCore.Server.Modules.World.Xml.Map;

[Serializable, XmlRoot(Namespace = "", ElementName = "map")]
public sealed class XmlMap
{
    [XmlElement("object")]
    public List<XmlObject> Objects { get; set; }
}
