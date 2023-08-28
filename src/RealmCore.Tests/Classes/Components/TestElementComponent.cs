using RealmCore.Server.Components.Elements.Abstractions;
using SlipeServer.Server.Elements;

namespace RealmCore.Tests.Classes.Components;

public class TestElement : Element
{

}

internal class TestElementComponent : ElementComponent
{
    private readonly TestElement _testElement;

    public TestElementComponent()
    {
        _testElement = new TestElement();
    }

    internal override Element Element => _testElement;
}
