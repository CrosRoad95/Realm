namespace RealmCore.Tests.Classes.Components;

public class TestElement : Element
{

}

internal class TestElementComponent : IComponent
{
    private Element _testElement;

    public TestElementComponent()
    {
        _testElement = new TestElement();
    }

    public Element Element { get => _testElement; set => _testElement = value; }
}
