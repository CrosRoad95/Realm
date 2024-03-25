namespace RealmCore.Tests.Unit.Players;

public class PlayerSessionsTests : RealmUnitTestingBase
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
    public void CreatingAndRemovingObjectiveShouldWork(bool withBlip, ElementType[] createdElementTypes)
    {
        var server = CreateServer();
        var player = CreatePlayer();

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
        var server = CreateServer();
        var player = CreatePlayer();

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
        var server = CreateServer();
        var player = CreatePlayer();

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
        var server = CreateServer();
        var player = CreatePlayer();

        var testJobSession = player.Sessions.BeginSession<TestJobSession>();
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
    public void BeginningSessionShouldTriggerAppropriateEvents()
    {
        var player = CreateServerWithOnePlayer();

        using var monitor = player.Sessions.Monitor();
        player.Sessions.BeginSession<TestSession>();
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["Started"]);

        player.Sessions.TryGetSession(out TestSession foundSession1).Should().BeTrue();
        player.Sessions.TryGetSession(out FailedToStartTestSession foundSession2).Should().BeFalse();
        player.Sessions.ToList().Should().BeEquivalentTo([foundSession1]);

        var getSession1 = () => player.Sessions.GetSession<TestSession>();
        var getSession2 = () => player.Sessions.GetSession<FailedToStartTestSession>();

        getSession1.Should().NotThrow();
        getSession2.Should().Throw<SessionNotFoundException>().Which.SessionType.Should().Be< FailedToStartTestSession>();
    }

    [Fact]
    public void SessionShouldNotBeAddedIfFailedToStart()
    {
        var player = CreateServerWithOnePlayer();

        var beginSession = () =>  player.Sessions.BeginSession<FailedToStartTestSession>();
        using var monitor = player.Sessions.Monitor();

        beginSession.Should().Throw<InvalidOperationException>();
        monitor.GetOccurredEvents().Should().BeEquivalentTo([]);
        player.Sessions.Count.Should().Be(0);
    }

    [Fact]
    public void OnlyOneSessionOfGivenTypeCanBeBegan()
    {
        var player = CreateServerWithOnePlayer();

        var beginSession = () => player.Sessions.BeginSession<TestSession>();

        var testSession = beginSession.Should().NotThrow().Subject;
        beginSession.Should().Throw<SessionAlreadyBegunException>();

        player.Sessions.Count.Should().Be(1);

        player.Sessions.IsDuringSession<TestSession>().Should().BeTrue();
        player.Sessions.IsDuringSession(testSession).Should().BeTrue();
    }

    [Fact]
    public void EndingSessionShouldWorkAsExpected1()
    {
        var player = CreateServerWithOnePlayer();

        var session = player.Sessions.BeginSession<TestSession>();

        var end = () => player.Sessions.EndSession(session);

        end.Should().NotThrow();
        end.Should().Throw<SessionNotFoundException>();

        session.EndedCount.Should().Be(1);
    }

    [Fact]
    public void EndingSessionShouldWorkAsExpected2()
    {
        var player = CreateServerWithOnePlayer();

        var session = player.Sessions.BeginSession<TestSession>();

        var end = () => player.Sessions.EndSession<TestSession>();

        end.Should().NotThrow();
        end.Should().Throw<SessionNotFoundException>();

        session.EndedCount.Should().Be(1);
    }

    [Fact]
    public void TryEndingSessionShouldWorkAsExpected1()
    {
        var player = CreateServerWithOnePlayer();

        player.Sessions.BeginSession<TestSession>();

        player.Sessions.TryEndSession<TestSession>().Should().BeTrue();
        player.Sessions.TryEndSession<TestSession>().Should().BeFalse();

        player.Sessions.Should().BeEmpty();
    }

    [Fact]
    public void TryEndingSessionShouldWorkAsExpected2()
    {
        var player = CreateServerWithOnePlayer();

        var session = player.Sessions.BeginSession<TestSession>();

        player.Sessions.TryEndSession(session).Should().BeTrue();
        player.Sessions.TryEndSession(session).Should().BeFalse();

        player.Sessions.Should().BeEmpty();
    }

    [Fact]
    public void SessionsShouldBeCleanedUpWhenPlayerDisconnect()
    {
        var player = CreateServerWithOnePlayer();

        var session = player.Sessions.BeginSession<TestSession>();
        using var monitorSession = session.Monitor();
        using var monitorSessions = player.Sessions.Monitor();
        player.TriggerDisconnected(QuitReason.Quit);

        player.Sessions.Should().BeEmpty();

        monitorSession.GetOccurredEvents().Should().BeEquivalentTo(["Ended"]);
        monitorSessions.GetOccurredEvents().Should().BeEquivalentTo(["Ended"]);
    }

    [Fact]
    public void SessionShouldMeasureTimeAppropriately()
    {
        var server = CreateServer();
        var player = CreatePlayer();

        var session = player.Sessions.BeginSession<TestSession>();

        server.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));
        session.TryStop();
        server.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));
        session.TryStart();
        server.DateTimeProvider.AddOffset(TimeSpan.FromSeconds(30));

        session.Elapsed.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void SessionCancellationTokenShouldBeCancelledWhenSessionEnd()
    {
        var player = CreateServerWithOnePlayer();

        var session = player.Sessions.BeginSession<TestSession>();
        var token = session.CreateCancellationToken();
        token.IsCancellationRequested.Should().BeFalse();

        player.Sessions.EndSession<TestSession>();
        token.IsCancellationRequested.Should().BeTrue();
    }
}
