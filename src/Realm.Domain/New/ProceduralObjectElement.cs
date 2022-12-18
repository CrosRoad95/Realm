using Realm.Assets.Factories;
using Realm.Resources.Assets;

namespace Realm.Domain.New;

public class ProceduralObjectElement : Component
{
    ProceduralObjectElement()
    {
    }

    public override Task Load()
    {
        var assetsService = Entity.GetRequiredService<AssetsService>();
        var modelFactory = new ModelFactory();
        modelFactory.AddTriangle(new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 10, 0));
        assetsService.AddModel("test", modelFactory.Build());
        return Task.CompletedTask;
    }
}
