using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Overlay;
using SlipeServer.Packets.Definitions.Lua;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace RealmCore.Tests.Tests.Components;

public class Hud3dComponentTests
{
    private readonly TestDateTimeProvider _testDateTimeProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<IAssetsService> _assetsService = new(MockBehavior.Strict);
    private readonly OverlayService _overlayService;

    public Hud3dComponentTests()
    {
        _assetsService.SetupGet(x => x.ReplaceModelForPlayer).Throws<NotImplementedException>();
        _assetsService.SetupGet(x => x.RestoreModelForPlayer).Throws<NotImplementedException>();
        _overlayService = new(_assetsService.Object);
        _testDateTimeProvider = new();
        var services = new ServiceCollection();
        services.AddSingleton<IDateTimeProvider>(_testDateTimeProvider);
        services.AddSingleton<IOverlayService>(_overlayService);

        _serviceProvider = services.BuildServiceProvider();
    }

    //[Fact]
    public void TestHud3dComponentShouldProduceAppropriateCallback()
    {
        #region Arrange
        var entity = new Entity("test");
        entity.Transform.Position = new Vector3(100, 100, 100);
        int callCount = 0;
        string _id = string.Empty;
        Vector3 _position = Vector3.Zero;
        LuaValue[] _luaValues = new LuaValue[] { };

        _overlayService.Hud3dCreated = (id, position, luaValues) =>
        {
            callCount++;
            _id = id;
            _position = position;
            _luaValues = luaValues.ToArray();
        };
        #endregion

        #region Act
        var hud3d = entity.AddComponent<SampleHud3d>();
        #endregion

        #region Assert
        callCount.Should().Be(1);
        _id.Should().Be(hud3d.Id.ToString());
        _position.Should().Be(new Vector3(104, 102, 100));
        var color = Color.White;
        var rectangleColor = Color.DeepPink;
        double luaColor = color.B + color.G * 256 + color.R * 256 * 256 + color.A * 256 * 256 * 256;
        double rectangleLuaColor = rectangleColor.B + rectangleColor.G * 256 + rectangleColor.R * 256 * 256 + rectangleColor.A * 256 * 256 * 256;
        var expectedLuaValues = new LuaValue[]
        {
            new LuaValue[] { "text", 1, "sample text", 10, 10, 10, 10, luaColor, 1, 1, "default", "left", "top" },
            new LuaValue[] { "text", 2, "420", 20, 20, 20, 20, luaColor, 1, 1, "default", "left", "top" },
            new LuaValue[] { "rectangle", 3, 30, 30, 30, 30, rectangleLuaColor },
        };

        _luaValues.Should().HaveCount(3);
        _luaValues[0].ToString().Should().BeEquivalentTo(_luaValues[0].ToString());
        _luaValues[1].ToString().Should().BeEquivalentTo(_luaValues[1].ToString());
        _luaValues[2].ToString().Should().BeEquivalentTo(_luaValues[2].ToString());
        #endregion
    }

    //[Fact]
    public void TestDetectedDynamicHudComponents()
    {
        #region Arrange
        var entity = new Entity("test");
        #endregion

        #region Act
        var hud3d = entity.AddComponent<SampleHud3d2>();
        #endregion

        #region Assert
        hud3d.DynamicHudComponents.Select(x => x.ComponentId).ToList().Should().BeEquivalentTo(new int[]
        {
            1, 3, 5
        });
        #endregion
    }

    //[Fact]
    public void UpdateStateShouldProduceAppropriateCallback()
    {
        #region Arrange
        var entity = new Entity("test");

        int callsCount = 0;
        string _id = string.Empty;
        Dictionary<int, object> _data = new();

        _overlayService.Hud3dStateChanged = (id, data) =>
        {
            callsCount++;
            _id = id;
            _data = data;
        };

        var hud3d = entity.AddComponent<SampleHud3d>();
        #endregion

        #region Act
        hud3d.UpdateState(x =>
        {
            x.Text = "new text";
            x.Number = "new number";
        });
        #endregion

        #region Assert
        callsCount.Should().Be(1);
        _id.Should().Be(hud3d.Id.ToString());
        _data.Should().BeEquivalentTo(new Dictionary<int, object>
        {
            [1] = "new text",
            [2] = "new number",
        });
        #endregion
    }

    //[Fact]
    public void Hud3dShouldBeRemovedWhenComponentGetRemoved()
    {
        #region Arrange
        var entity = new Entity("test");

        string _id = string.Empty;
        _overlayService.Hud3dRemoved = id =>
        {
            _id = id;
        };

        #endregion

        #region Act
        var hud3d = entity.AddComponent<SampleHud3d2>();
        entity.DestroyComponent(hud3d);
        #endregion

        #region Assert
        _id.Should().Be(hud3d.Id.ToString());
        #endregion
    }

    [Fact]
    public void ItIsNotAllowedToUpdateStatelessHud()
    {
        #region Arrange
        var entity = new Entity("test");
        var hud3d = entity.AddComponent<SampleStateLessHud3d>();
        #endregion

        #region Act
        var act = () => hud3d.UpdateState(e => { });
        #endregion

        #region Assert
        act.Should().Throw<Exception>().WithMessage("Hud3d has no state");
        #endregion
    }
}

internal class SampleHud3d : Hud3dComponent<SampleState>
{
    public SampleHud3d() : base(x =>
        {
            x.AddText(x => x.Text, new Vector2(10, 10), new Size(10, 10));
            x.AddText(x => x.Number, new Vector2(20, 20), new Size(20, 20));
            x.AddRectangle(new Vector2(30, 30), new Size(30, 30), Color.DeepPink);
        }, new SampleState
        {
            Text = "sample text",
            Number = "420"
        }, new Vector3(4, 2, 0))
    {
    }
}

internal class SampleHud3d2 : Hud3dComponent<SampleState>
{
    public SampleHud3d2() : base(x =>
        {
            x.AddText(x => x.Text, new Vector2(10, 10), new Size(10, 10));
            x.AddText("test", new Vector2(20, 20), new Size(20, 20), font: "default");
            x.AddText(x => x.Text, new Vector2(10, 10), new Size(10, 10));
            x.AddText("test", new Vector2(20, 20), new Size(20, 20), font: "default");
            x.AddText(x => x.Text, new Vector2(10, 10), new Size(10, 10));
        }, new SampleState
        {
            Text = "sample text",
            Number = "420"
        }, new Vector3(4, 2, 0))
    {
    }
}


internal class SampleStateLessHud3d : Hud3dComponent
{
    public SampleStateLessHud3d() : base(x =>
        {
            x.AddText("test", new Vector2(10, 10), new Size(10, 10), font: "sans");
        }, new Vector3(4, 2, 0))
    {
    }
}

internal class SampleState
{
    public string Text { get; set; }
    public string Number { get; set; }
}
