using Microsoft.ClearScript;

namespace Realm.Domain.New;

public sealed class TestComponent : Component
{
    private readonly string _foo;

    [ScriptMember("foo")]
    public string Foo => _foo;

    public TestComponent(string foo)
    {
        _foo = foo;
    }

    public override string ToString() => _foo;
}
