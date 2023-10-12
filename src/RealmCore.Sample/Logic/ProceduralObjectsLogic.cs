using RealmCore.Resources.Assets.Factories;
using RealmCore.Resources.Assets;
using SlipeServer.Server.Enums;

namespace RealmCore.Sample.Logic;

internal class ProceduralObjectsLogic
{
    public static byte[] ReadFully(Stream input)
    {
        byte[] buffer = new byte[16 * 1024];
        using (MemoryStream ms = new())
        {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            input.Position = 0;
            return ms.ToArray();
        }
    }

    public ProceduralObjectsLogic(AssetsRegistry assetsRegistry)
    {
        var modelFactory = new ModelFactory();
        modelFactory.AddTriangle(new Vector3(2, 2, 0), new Vector3(0, 10, 0), new Vector3(10, 0, 0), "Metal1_128");
        modelFactory.AddTriangle(new Vector3(0, 10, 0), new Vector3(10, 0, 0), new Vector3(10, 10, 0), "Metal1_128");
        var dff = modelFactory.BuildDff();
        var col = modelFactory.BuildCol();
#if DEBUG
        if (!Directory.Exists("testoutput"))
            Directory.CreateDirectory("testoutput");
        File.WriteAllBytes("testoutput/debugmodel.dff", ReadFully(dff));
        File.WriteAllBytes("testoutput/debugmodel.col", ReadFully(col));
#endif
        var model = assetsRegistry.AddModel("test", col, dff);
        assetsRegistry.ReplaceModel((ObjectModel)1338, model);
        ;
    }
}
