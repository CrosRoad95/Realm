namespace RealmCore.Tests.Unit.Players;

public class HudTests : RealmUnitTestingBase
{
    private class SampleLayer : HudLayer
    {
        public bool Disposed { get; set; } = false;
        protected override void Build(IHudBuilder<object> hudBuilderCallback, IHudBuilderContext hudBuilderContext)
        {
        }

        public override void Dispose()
        {
            Disposed = true;
        }
    }

    [Fact]
    public void RemovingLayerShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();

        var layer = new SampleLayer();

        using var monitor = player.Hud.Monitor();
        player.Hud.AddLayer(layer);
        player.Hud.RemoveLayer<SampleLayer>();

        layer.Disposed.Should().BeTrue();
        player.Hud.Layers.Should().HaveCount(0);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["LayerAdded", "LayerRemoved"]);
    }

    [Fact]
    public void DisconnectingPlayerShouldRemoveAllHudLayers()
    {
        var server = CreateServer();
        var player = CreatePlayer();

        var layer = new SampleLayer();

        using var monitor = player.Hud.Monitor();
        player.Hud.AddLayer(layer);
        player.TriggerDisconnected(QuitReason.Quit);

        layer.Disposed.Should().BeTrue();
        player.Hud.Layers.Should().HaveCount(0);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["LayerAdded", "LayerRemoved"]);
    }
}