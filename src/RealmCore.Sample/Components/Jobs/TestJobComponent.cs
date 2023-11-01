namespace RealmCore.Sample.Components.Jobs;

internal class TestJobComponent : JobSessionComponent
{
    private readonly IElementFactory _elementFactory;

    public override short JobId => 1;

    public TestJobComponent(IElementFactory entityFactory)
    {
        _elementFactory = entityFactory;
    }

    public void CreateObjectives()
    {
        var objective = AddObjective(new MarkerEnterObjective(new Vector3(383.6543f, -82.01953f, 3.914598f)));
        objective.AddBlip(BlipIcon.North);
        objective.Completed += ObjectiveACompleted;

        var objectEntity = _elementFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, new Vector3(379.00f, -102.77f, 1.24f), Vector3.Zero);
        objectEntity.AddComponent<LiftableWorldObjectComponent>();
        objectEntity.AddComponent(new OwnerDisposableComponent(Element));
        var objective2 = AddObjective(new TransportObjectObjective(objectEntity, new Vector3(379.00f, -112.77f, 2.0f)));
        objective2.Completed += ObjectiveBCompleted;
        var objective3 = AddObjective(new TransportObjectObjective(new Vector3(379.00f, -105.77f, 2.0f)));
        objective3.Completed += ObjectiveDCompleted;

        var subObjective1 = new MarkerEnterObjective(new Vector3(386.9004f, -89.74414f, 3.8843315f));
        var subObjective2 = new MarkerEnterObjective(new Vector3(393.80566f, -99.60156f, 5.2993988f));
        var objective1 = AddObjective(new OneOfObjective(subObjective1, subObjective2));
        objective1.Completed += ObjectiveCCompleted;
    }

    private void ObjectiveACompleted(Objective objective, object? data = null)
    {
        ((IComponents)Element).GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 1);
        //ChatBox.OutputTo(Entity, $"Entered marker, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveBCompleted(Objective objective, object? data = null)
    {
        ((IComponents)Element).GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 2);
        //ChatBox.OutputTo(Entity, $"Box delivered, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveCCompleted(Objective objective, object? data = null)
    {
        ((IComponents)Element).GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 1);
        //ChatBox.OutputTo(Entity, $"Entered one of marker, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveDCompleted(Objective objective, object? data = null)
    {
        //ChatBox.OutputTo(Entity, $"Entered marker, objectives left: {Objectives.Count()} {data}");
    }
}
