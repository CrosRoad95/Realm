namespace Realm.Domain.Components.Elements;

public class ProceduralObjectElementComponent : ElementComponent
{
    [Inject]
    private AssetsService AssetsService { get; set; } = default!;

    internal override Element Element => throw new NotImplementedException();

    internal ProceduralObjectElementComponent()
    {
    }

    public override Task LoadAsync()
    {
        var modelFactory = new ModelFactory();
        modelFactory.AddTriangle(new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 10, 0));
        AssetsService.AddModel("test", modelFactory.Build());
        return Task.CompletedTask;
    }
}
