using Realm.Assets.Factories;
using Realm.Resources.Assets;

namespace Realm.Server.Logic;

internal class AssetsLogic
{
    public AssetsLogic(AssetsService assetsService)
    {
        var modelFactory = new ModelFactory();
        modelFactory.AddTriangle(new Vector3(0, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 10, 0));
        assetsService.AddModel("test", modelFactory.Build());
    }
}
