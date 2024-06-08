namespace RealmCore.Tests.Unit.Players;

public class HudTests
{
    private class SampleLayer : HudLayer
    {
        public bool Disposed { get; set; } = false;
        protected override void Build(IHudBuilder hudBuilderCallback, IHudBuilderContext hudBuilderContext)
        {
        }

        public override void Dispose()
        {
            Disposed = true;
        }
    }

    [Fact]
    public async Task RemovingLayerShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var layer = new SampleLayer();

        using var monitor = player.Hud.Monitor();
        player.Hud.AddLayer(layer);
        player.Hud.RemoveLayer<SampleLayer>();

        layer.Disposed.Should().BeTrue();
        player.Hud.Should().HaveCount(0);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["LayerAdded", "LayerRemoved"]);
    }

    [Fact]
    public async Task DisconnectingPlayerShouldRemoveAllHudLayers()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var layer = new SampleLayer();

        using var monitor = player.Hud.Monitor();
        player.Hud.AddLayer(layer);
        await hosting.DisconnectPlayer(player);

        layer.Disposed.Should().BeTrue();
        player.Hud.Should().HaveCount(0);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["LayerAdded", "LayerRemoved"]);
    }
}