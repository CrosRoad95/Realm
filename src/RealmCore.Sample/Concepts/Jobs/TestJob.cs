namespace RealmCore.Sample.Concepts.Jobs;

internal class TestJob : JobSession
{
    public TestJob(IScopedElementFactory scopedElementFactory, PlayerContext playerContext, IPeriodicEventDispatcher updateService) : base(scopedElementFactory, playerContext, updateService)
    {
    }

    public override short JobId => 1;

    public void CreateObjectives()
    {
        var objective = AddObjective(new MarkerEnterObjective(new Vector3(383.6543f, -82.01953f, 3.914598f)));
        objective.AddBlip(BlipIcon.North);
        objective.Completed += ObjectiveACompleted;

        var worldObject = _elementFactory.CreateObject(ObjectModel.Gunbox, new Vector3(379.00f, -102.77f, 1.24f), Vector3.Zero);
        worldObject.Interaction = new LiftableInteraction();
        worldObject.TrySetOwner(Player);
        var objective2 = AddObjective(new TransportObjectObjective(worldObject, new Vector3(379.00f, -112.77f, 2.0f)));
        objective2.Completed += ObjectiveBCompleted;
        var objective3 = AddObjective(new TransportObjectObjective(new Vector3(379.00f, -105.77f, 2.0f), 2, true));
        objective3.Completed += ObjectiveDCompleted;

        var subObjective1 = new MarkerEnterObjective(new Vector3(386.9004f, -89.74414f, 3.8843315f));
        var subObjective2 = new MarkerEnterObjective(new Vector3(393.80566f, -99.60156f, 5.2993988f));
        var objective1 = AddObjective(new OneOfObjective(subObjective1, subObjective2));
        objective1.Completed += ObjectiveCCompleted;
    }

    private void ObjectiveACompleted(Objective objective, object? data = null)
    {
        Player.JobStatistics.AddPoints(1, 1);
        //ChatBox.OutputTo(player, $"Entered marker, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveBCompleted(Objective objective, object? data = null)
    {
        Player.JobStatistics.AddPoints(1, 2);
        //ChatBox.OutputTo(player, $"Box delivered, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveCCompleted(Objective objective, object? data = null)
    {
        Player.JobStatistics.AddPoints(1, 1);
        //ChatBox.OutputTo(player, $"Entered one of marker, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveDCompleted(Objective objective, object? data = null)
    {
        //ChatBox.OutputTo(player, $"Entered marker, objectives left: {Objectives.Count()} {data}");
    }
}
