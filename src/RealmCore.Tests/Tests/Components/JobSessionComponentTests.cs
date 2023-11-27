using RealmCore.Server.Components.Players.Jobs;
using RealmCore.Server.Concepts.Objectives;
using RealmCore.Server.Contexts;
using SlipeServer.Server.Enums;

namespace RealmCore.Tests.Tests.Components;

public class JobSessionComponentTests
{
    internal class TestJobSession : JobSession
    {
        public TestJobSession(IScopedElementFactory scopedElementFactory, PlayerContext playerContext, IUpdateService updateService) : base(scopedElementFactory, playerContext, updateService)
        {
        }

        public override short JobId => 1;
        public int ObjectiveCount => Objectives.Count();

        public Objective CreateMarkerObjective(bool withBlip = true)
        {
            var objective = AddObjective(new MarkerEnterObjective(new Vector3(383.6543f, -82.01953f, 3.914598f)));
            if (withBlip)
                objective.AddBlip(BlipIcon.North);
            return objective;
        }

        public Objective CreateOneOfObjective()
        {
            var marker1 = new MarkerEnterObjective(new Vector3(400.0f, -82.01953f, 3.914598f));
            var marker2 = new MarkerEnterObjective(new Vector3(500.0f, -82.01953f, 3.914598f));
            var objective = AddObjective(new OneOfObjective(marker1, marker2));
            return objective;
        }

        public Objective CreateTransportObjectObjective1(Element? element = null)
        {
            Objective objective;
            if (element != null)
            {
                objective = AddObjective(new TransportObjectObjective(element, new Vector3(400.0f, -82.01953f, 3.914598f)));
            }
            else
            {
                objective = AddObjective(new TransportObjectObjective(new Vector3(400.0f, -82.01953f, 3.914598f)));
            }
            return objective;
        }
    }

    [InlineData(true, new ElementType[] { ElementType.Marker, ElementType.Blip })]
    [InlineData(false, new ElementType[] { ElementType.Marker })]
    [Theory]
    public void CreatingAndRemovingObjectiveShouldWork(bool withBlip, ElementType[] createdElementTypes)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var testJobSession = player.Sessions.BeginSession<TestJobSession>();
        var objective = testJobSession.CreateMarkerObjective(withBlip);
        var elements = player.ElementFactory.CreatedElements.ToList();
        elements.Select(x => x.ElementType).Should().BeEquivalentTo(createdElementTypes);
        testJobSession.ObjectiveCount.Should().Be(1);
        objective.Dispose();
        elements = player.ElementFactory.CreatedElements.ToList();
        testJobSession.ObjectiveCount.Should().Be(0);
        elements.Count.Should().Be(0);
    }

    [Fact]
    public void MarkerObjectiveShouldBeCompletable()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var testJobSession = player.Sessions.BeginSession<TestJobSession>();
        var objective = testJobSession.CreateMarkerObjective(false);

        bool completed = false;
        void handleCompleted(Objective arg1, object? arg2)
        {
            completed = true;
        }
        objective.Completed += handleCompleted;
        player.Position = new Vector3(383.6543f, -82.01953f, 3.914598f);
        completed.Should().BeTrue();
        testJobSession.ObjectiveCount.Should().Be(0);
    }

    [InlineData(400.0f, -82.01953f, 3.914598f, 0)]
    [InlineData(500.0f, -82.01953f, 3.914598f, 0)]
    [InlineData(600.0f, -82.01953f, 3.914598f, 1)]
    [Theory]
    public void OneOfObjectiveShouldBeCompletable(float x, float y, float z, int expectedObjectiveCount)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var testJobSession = player.Sessions.BeginSession<TestJobSession>();
        var objective = testJobSession.CreateOneOfObjective();

        bool completed = false;
        void handleCompleted(Objective arg1, object? arg2)
        {
            completed = true;
        }
        objective.Completed += handleCompleted;
        player.Position = new Vector3(x, y, z);
        testJobSession.ObjectiveCount.Should().Be(expectedObjectiveCount);
        completed.Should().Be(expectedObjectiveCount == 0);
    }

    [Fact]
    public void CreateTransportObjectObjectiveShouldBeCompletable()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var testJobSession = player.Sessions.BeginSession<TestJobSession>();
        var myObject = player.ElementFactory.CreateObject((ObjectModel)1337, new Vector3(100, 100, 100), Vector3.Zero);
        myObject.AddComponent(new OwnerComponent(player));
        var objective = testJobSession.CreateTransportObjectObjective1();

        bool completed = false;
        void handleCompleted(Objective arg1, object? arg2)
        {
            arg2.Should().Be(myObject);
            completed = true;
        }
        objective.Completed += handleCompleted;

        var destination = new Vector3(400.0f, -82.01953f, 3.914598f);
        myObject.Position = destination;
        realmTestingServer.ForceUpdate();
        completed.Should().Be(true);
        testJobSession.ObjectiveCount.Should().Be(0);
    }
}
