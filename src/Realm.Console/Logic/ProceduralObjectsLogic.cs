using Realm.Resources.Assets;
using Realm.Resources.Assets.Factories;
using SlipeServer.Server.Enums;

namespace Realm.Console.Logic;

internal class ProceduralObjectsLogic
{
    public ProceduralObjectsLogic(AssetsRegistry assetsRegistry)
    {
        var modelFactory = new ModelFactory();
        modelFactory.AddTriangle(new Vector3(2, 2, 0), new Vector3(0, 10, 0), new Vector3(10, 0, 0));
        modelFactory.AddTriangle(new Vector3(0, 10, 0), new Vector3(10, 0, 0), new Vector3(10, 10, 0));
        var model = assetsRegistry.AddModel("test", modelFactory.BuildCol(), modelFactory.BuildDff());
        assetsRegistry.ReplaceModel((ObjectModel)1338, model);
        ;
    }
}
