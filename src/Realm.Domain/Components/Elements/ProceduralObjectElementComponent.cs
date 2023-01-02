using Realm.Assets.Factories;
using Realm.Resources.Assets;

namespace Realm.Domain.Components.Elements;

public class ProceduralObjectElementComponent : ElementComponent
{
    [Inject]
    private AssetsService AssetsService { get; set; } = default!;

    public override Element Element => throw new NotImplementedException();

    ProceduralObjectElementComponent()
    {
    }

    public override Task Load()
    {
        var modelFactory = new ModelFactory();
        modelFactory.AddTriangle(new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 10, 0));
        AssetsService.AddModel("test", modelFactory.Build());
        return Task.CompletedTask;
    }
}
