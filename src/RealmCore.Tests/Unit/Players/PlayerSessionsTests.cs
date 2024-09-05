namespace RealmCore.Tests.Unit.Players;

public class PlayerSessionsTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerSessionsFeature _sessions;

    public PlayerSessionsTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
        _sessions = _player.Sessions;
    }

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
    public void CreatingAndRemovingObjectiveShouldWork(bool withBlip, ElementType[] createdElementTypes)
    {
        var testJobSession = _sessions.Begin<TestJobSession>();
        var objective = testJobSession.CreateMarkerObjective(withBlip);
        var elements = _player.ElementFactory.CreatedElements.ToList();
        elements.Select(x => x.ElementType).Should().BeEquivalentTo(createdElementTypes);
        testJobSession.ObjectiveCount.Should().Be(1);
        objective.Dispose();
        elements = _player.ElementFactory.CreatedElements.ToList();

        testJobSession.ObjectiveCount.Should().Be(0);
        elements.Count.Should().Be(0);
    }

    [Fact]
    public void MarkerObjectiveShouldBeCompletable()
    {
        var testJobSession = _sessions.Begin<TestJobSession>();
        var objective = testJobSession.CreateMarkerObjective(false);

        bool completed = false;
        void handleCompleted(Objective arg1, object? arg2)
        {
            completed = true;
        }
        objective.Completed += handleCompleted;
        _player.Position = new Vector3(383.6543f, -82.01953f, 3.914598f);
        completed.Should().BeTrue();
        testJobSession.ObjectiveCount.Should().Be(0);
    }

    [InlineData(400.0f, -82.01953f, 3.914598f, 0)]
    [InlineData(500.0f, -82.01953f, 3.914598f, 0)]
    [InlineData(600.0f, -82.01953f, 3.914598f, 1)]
    [Theory]
    public void OneOfObjectiveShouldBeCompletable(float x, float y, float z, int expectedObjectiveCount)
    {
        var testJobSession = _sessions.Begin<TestJobSession>();
        var objective = testJobSession.CreateOneOfObjective();

        bool completed = false;
        void handleCompleted(Objective arg1, object? arg2)
        {
            completed = true;
        }
        objective.Completed += handleCompleted;
        _player.Position = new Vector3(x, y, z);
        testJobSession.ObjectiveCount.Should().Be(expectedObjectiveCount);
        completed.Should().Be(expectedObjectiveCount == 0);
    }

    [Fact]
    public void CreateTransportObjectObjectiveShouldBeCompletable()
    {
        var testJobSession = _sessions.Begin<TestJobSession>();
        var myObject = _player.ElementFactory.CreateObject(new Location(new Vector3(100, 100, 100), Vector3.Zero), (ObjectModel)1337);
        myObject.TrySetOwner(_player);
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
    public void BeginningSessionShouldTriggerAppropriateEvents()
    {
        using var monitor = _sessions.Monitor();
        _sessions.Begin<TestSession>();
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["Started"]);

        _sessions.TryGet(out TestSession foundSession1).Should().BeTrue();
        _sessions.TryGet(out FailedToStartTestSession foundSession2).Should().BeFalse();
        _sessions.ToList().Should().BeEquivalentTo([foundSession1]);

        var getSession1 = () => _sessions.Get<TestSession>();
        var getSession2 = () => _sessions.Get<FailedToStartTestSession>();

        getSession1.Should().NotThrow();
        getSession2.Should().Throw<SessionNotFoundException>().Which.SessionType.Should().Be< FailedToStartTestSession>();
    }

    [Fact]
    public void SessionShouldNotBeAddedIfFailedToStart()
    {
        var beginSession = () => _sessions.Begin<FailedToStartTestSession>();
        using var monitor = _sessions.Monitor();

        beginSession.Should().Throw<InvalidOperationException>();
        monitor.GetOccurredEvents().Should().BeEquivalentTo([]);
        _sessions.Count.Should().Be(0);
    }

    [Fact]
    public void OnlyOneSessionOfGivenTypeCanBeBegan()
    {
        var beginSession = () => _sessions.Begin<TestSession>();

        var testSession = beginSession.Should().NotThrow().Subject;
        beginSession.Should().Throw<SessionAlreadyBegunException>();

        _sessions.Count.Should().Be(1);

        _sessions.IsDuring<TestSession>().Should().BeTrue();
        _sessions.IsDuring(testSession).Should().BeTrue();
    }

    [Fact]
    public void EndingSessionShouldWorkAsExpected1()
    {
        var session = _sessions.Begin<TestSession>();

        _sessions.TryEnd(session).Should().BeTrue();
        _sessions.TryEnd(session).Should().BeFalse();

        session.EndedCount.Should().Be(1);
    }

    [Fact]
    public void EndingSessionShouldWorkAsExpected2()
    {
        var session = _sessions.Begin<TestSession>();

        var end = () => _sessions.TryEnd<TestSession>();

        _sessions.TryEnd<TestSession>().Should().BeTrue();
        _sessions.TryEnd<TestSession>().Should().BeFalse();

        session.EndedCount.Should().Be(1);
    }

    [Fact]
    public void TryEndingSessionShouldWorkAsExpected1()
    {
        _sessions.Begin<TestSession>();

        _sessions.TryEnd<TestSession>().Should().BeTrue();
        _sessions.TryEnd<TestSession>().Should().BeFalse();

        _sessions.Should().BeEmpty();
    }

    [Fact]
    public void TryEndingSessionShouldWorkAsExpected2()
    {
        var session = _sessions.Begin<TestSession>();

        _sessions.TryEnd(session).Should().BeTrue();
        _sessions.TryEnd(session).Should().BeFalse();

        _sessions.Should().BeEmpty();
    }

    [Fact]
    public async Task SessionsShouldBeCleanedUpWhenPlayerDisconnect()
    {
        var player = await _hosting.CreatePlayer();
        var session = player.Sessions.Begin<TestSession>();
        using var monitorSession = session.Monitor();
        using var monitorSessions = player.Sessions.Monitor();

        await _hosting.DisconnectPlayer(player);

        player.Sessions.ToArray().Should().BeEmpty();

        monitorSession.GetOccurredEvents().Should().BeEquivalentTo(["Ended"]);
        monitorSessions.GetOccurredEvents().Should().BeEquivalentTo(["Ended"]);
    }

    [Fact]
    public void SessionShouldMeasureTimeAppropriately()
    {
        var session = _sessions.Begin<TestSession>();

        _hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));
        session.TryStop();
        _hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));
        session.TryStart();
        _hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));

        session.Elapsed.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void SessionCancellationTokenShouldBeCancelledWhenSessionEnd()
    {
        var session = _sessions.Begin<TestSession>();
        var token = session.CreateCancellationToken();
        token.IsCancellationRequested.Should().BeFalse();

        _sessions.TryEnd<TestSession>();
        token.IsCancellationRequested.Should().BeTrue();
    }

    public void Dispose()
    {
        _sessions.Clear();
    }
}
