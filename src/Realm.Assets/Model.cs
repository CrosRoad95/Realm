using Realm.Resources.Assets;

namespace Realm.Assets;

internal class Model : IModel
{
    public byte[] Dff { get; set; }

    public byte[] Col { get; set; }

    public Model(byte[] dff, byte[] col)
    {
        Dff = dff;
        Col = col;
    }
}
