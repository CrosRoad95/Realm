namespace RealmCore.Tests.Unit.Players;

public class PlayerSessionsTests
{
    internal sealed class TestSession : Session
    {
        public override string Name => "Test";
        public int EndedCount { get; private set; }

        public TestSession(PlayerContext playerContext, IDateTimeProvider dateTimeProvider) : base(playerContext.Player, dateTimeProvider)
        {
        }

        protected override void OnEnded()
        {
            EndedCount++;
        }
    }

    internal sealed class FailedToStartTestSession : Session
    {
        public override string Name => "FailedToStart";
        public FailedToStartTestSession(PlayerContext playerContext, IDateTimeProvider dateTimeProvider) : base(playerContext.Player, dateTimeProvider)
        {
        }

        protected override void OnStarted()
        {
            throw new InvalidOperationException(); // Exception for test purpose
        }
    }

    internal sealed class TestJobSession : JobSession
    {
        public override string Name => "TestJob";
        public TestJobSession(PlayerContext playerContext, IDateTimeProvider dateTimeProvider) : base(playerContext, dateTimeProvider)
        {
        }

        public override short JobId => 1;
        public int ObjectiveCount => Objectives.Count();

        public Objective CreateMarkerObjective(bool withBlip = true)
        {
            var objective = AddObjective(new MarkerEnterObjective(new Location(383.6543f, -82.01953f, 3.914598f)));
            if (withBlip)
                objective.AddBlip(BlipIcon.North);
            return objective;
        }

        public Objective CreateOneOfObjective()
        {
            var marker1 = new MarkerEnterObjective(new Location(400.0f, -82.01953f, 3.914598f));
            var marker2 = new MarkerEnterObjective(new Location(500.0f, -82.01953f, 3.914598f));
            var objective = AddObjective(new OneOfObjective(marker1, marker2));
            return objective;
        }

        public Objective CreateTransportObjectObjective1(Element? element = null)
        {
            Objective objective;
            if (element != null)
            {
                objective = AddObjective(new TransportObjectObjective(element, new Location(400.0f, -82.01953f, 3.914598f)));
            }
            else
            {
                objective = AddObjective(new TransportObjectObjective(new Location(400.0f, -82.01953f, 3.914598f)));
            }
            return objective;
        }
    }

    [InlineData(true, new ElementType[] { ElementType.Marker, ElementType.Blip })]
    [InlineData(false, new ElementType[] { ElementType.Marker })]
    [Theory]
    public async Task CreatingAndRemovingObjectiveShouldWork(bool withBlip, ElementType[] createdElementTypes)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var testJobSession = player.Sessions.Begin<TestJobSession>();
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
    public async Task MarkerObjectiveShouldBeCompletable()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var testJobSession = player.Sessions.Begin<TestJobSession>();
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
    public async Task OneOfObjectiveShouldBeCompletable(float x, float y, float z, int expectedObjectiveCount)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var testJobSession = player.Sessions.Begin<TestJobSession>();
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
    public async Task CreateTransportObjectObjectiveShouldBeCompletable()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var testJobSession = player.Sessions.Begin<TestJobSession>();
        var myObject = player.ElementFactory.CreateObject(new Location(new Vector3(100, 100, 100), Vector3.Zero), (ObjectModel)1337);
        myObject.TrySetOwner(player);
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
        objective.Update();
        completed.Should().Be(true);
        testJobSession.ObjectiveCount.Should().Be(0);
    }

    [Fact]
    public async Task BeginningSessionShouldTriggerAppropriateEvents()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        using var monitor = player.Sessions.Monitor();
        player.Sessions.Begin<TestSession>();
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["Started"]);

        player.Sessions.TryGet(out TestSession foundSession1).Should().BeTrue();
        player.Sessions.TryGet(out FailedToStartTestSession foundSession2).Should().BeFalse();
        player.Sessions.ToList().Should().BeEquivalentTo([foundSession1]);

        var getSession1 = () => player.Sessions.Get<TestSession>();
        var getSession2 = () => player.Sessions.Get<FailedToStartTestSession>();

        getSession1.Should().NotThrow();
        getSession2.Should().Throw<SessionNotFoundException>().Which.SessionType.Should().Be< FailedToStartTestSession>();
    }

    [Fact]
    public async Task SessionShouldNotBeAddedIfFailedToStart()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var beginSession = () =>  player.Sessions.Begin<FailedToStartTestSession>();
        using var monitor = player.Sessions.Monitor();

        beginSession.Should().Throw<InvalidOperationException>();
        monitor.GetOccurredEvents().Should().BeEquivalentTo([]);
        player.Sessions.Count.Should().Be(0);
    }

    [Fact]
    public async Task OnlyOneSessionOfGivenTypeCanBeBegan()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var beginSession = () => player.Sessions.Begin<TestSession>();

        var testSession = beginSession.Should().NotThrow().Subject;
        beginSession.Should().Throw<SessionAlreadyBegunException>();

        player.Sessions.Count.Should().Be(1);

        player.Sessions.IsDuring<TestSession>().Should().BeTrue();
        player.Sessions.IsDuring(testSession).Should().BeTrue();
    }

    [Fact]
    public async Task EndingSessionShouldWorkAsExpected1()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var session = player.Sessions.Begin<TestSession>();

        player.Sessions.TryEnd(session).Should().BeTrue();
        player.Sessions.TryEnd(session).Should().BeFalse();

        session.EndedCount.Should().Be(1);
    }

    [Fact]
    public async Task EndingSessionShouldWorkAsExpected2()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var session = player.Sessions.Begin<TestSession>();

        var end = () => player.Sessions.TryEnd<TestSession>();

        player.Sessions.TryEnd<TestSession>().Should().BeTrue();
        player.Sessions.TryEnd<TestSession>().Should().BeFalse();

        session.EndedCount.Should().Be(1);
    }

    [Fact]
    public async Task TryEndingSessionShouldWorkAsExpected1()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        player.Sessions.Begin<TestSession>();

        player.Sessions.TryEnd<TestSession>().Should().BeTrue();
        player.Sessions.TryEnd<TestSession>().Should().BeFalse();

        player.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task TryEndingSessionShouldWorkAsExpected2()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var session = player.Sessions.Begin<TestSession>();

        player.Sessions.TryEnd(session).Should().BeTrue();
        player.Sessions.TryEnd(session).Should().BeFalse();

        player.Sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task SessionsShouldBeCleanedUpWhenPlayerDisconnect()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var session = player.Sessions.Begin<TestSession>();
        using var monitorSession = session.Monitor();
        using var monitorSessions = player.Sessions.Monitor();
        await hosting.DisconnectPlayer(player);

        player.Sessions.ToArray().Should().BeEmpty();

        monitorSession.GetOccurredEvents().Should().BeEquivalentTo(["Ended"]);
        monitorSessions.GetOccurredEvents().Should().BeEquivalentTo(["Ended"]);
    }

    [Fact]
    public async Task SessionShouldMeasureTimeAppropriately()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var session = player.Sessions.Begin<TestSession>();

        hosting.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));
        session.TryStop();
        hosting.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));
        session.TryStart();
        hosting.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));

        session.Elapsed.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task SessionCancellationTokenShouldBeCancelledWhenSessionEnd()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var session = player.Sessions.Begin<TestSession>();
        var token = session.CreateCancellationToken();
        token.IsCancellationRequested.Should().BeFalse();

        player.Sessions.TryEnd<TestSession>();
        token.IsCancellationRequested.Should().BeTrue();
    }
}
