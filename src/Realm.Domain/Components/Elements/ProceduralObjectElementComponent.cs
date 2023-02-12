using Realm.Assets.Factories;
using Realm.Resources.Assets;

namespace Realm.Domain.Components.Elements;

public class ProceduralObjectElementComponent : ElementComponent
{
    [Inject]
    private AssetsRegistry AssetsRegistry { get; set; } = default!;

    internal override Element Element => throw new NotImplementedException();

    internal ProceduralObjectElementComponent()
    {
    }

    protected override Task LoadAsync()
    {
        var modelFactory = new ModelFactory();
        modelFactory.AddTriangle(new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 10, 0));
        AssetsRegistry.AddModel("test", modelFactory.Build());
        return Task.CompletedTask;
    }
}
