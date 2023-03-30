using RenderWareBuilders;
using System.Drawing;
using System.Numerics;

namespace Realm.Resources.Assets.Factories;

public class ModelFactory
{
    private readonly RenderWareBuilder _renderWareBuilder;
    public ModelFactory()
    {
        _renderWareBuilder = new RenderWareBuilder();
    }

    public void AddTriangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        var vert1 = _renderWareBuilder.AddVertex(vertex1, Vector3.UnitZ, new Vector2(vertex1.X, vertex1.Y));
        var vert2 = _renderWareBuilder.AddVertex(vertex2, Vector3.UnitZ, new Vector2(vertex2.X, vertex2.Y));
        var vert3 = _renderWareBuilder.AddVertex(vertex3, Vector3.UnitZ, new Vector2(vertex3.X, vertex3.Y));
        var mat = _renderWareBuilder.AddMaterial(new Material
        {
            Color = Color.White,
            MaskName = "",
            Name = "Metal1_128",
        });
        _renderWareBuilder.AddTriangle(new Triangle
        {
            Vertex1 = vert1,
            Vertex2 = vert2,
            Vertex3 = vert3,
            Material = mat
        });
    }

    public Stream BuildDff()
    {
        var dff = _renderWareBuilder.BuildDff();
        MemoryStream stream = new();
        dff.Write(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public Stream BuildCol()
    {
        var col = _renderWareBuilder.BuildCol();
        MemoryStream stream = new();
        col.Write(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

}
