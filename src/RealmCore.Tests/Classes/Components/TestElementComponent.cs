namespace RealmCore.Tests.Classes.Components;

public class TestElement : Element
{

}

internal class TestElementComponent : IComponent
{
    public Element TestElement { get; set; }
    public Element Element { get; set; }

    public TestElementComponent()
    {
        TestElement = new TestElement();
    }
}
