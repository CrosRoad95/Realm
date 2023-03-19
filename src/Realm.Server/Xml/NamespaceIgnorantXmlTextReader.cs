using System.Xml;

namespace Realm.Server.Xml;

internal class NamespaceIgnorantXmlTextReader : XmlTextReader
{
    public NamespaceIgnorantXmlTextReader(Stream stream) : base(stream) { }

    public override string NamespaceURI
    {
        get { return ""; }
    }
}
