namespace RealmCore.Console.Components.Jobs;

internal class TestJobComponent : JobSessionComponent
{
    public override short JobId => 1;

    public TestJobComponent()
    {

    }

    public void CreateObjectives()
    {
        var objective = AddObjective(new MarkerEnterObjective(new Vector3(383.6543f, -82.01953f, 3.914598f)));
        objective.AddBlip(BlipIcon.North, EntityFactory);
        objective.Completed += ObjectiveACompleted;

        var objectEntity = EntityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, new Vector3(379.00f, -102.77f, 1.24f), Vector3.Zero);
        objectEntity.AddComponent<LiftableWorldObjectComponent>();
        var objective2 = AddObjective(new TransportEntityObjective(objectEntity, new Vector3(379.00f, -112.77f, 2.0f)));
        objective2.Completed += ObjectiveBCompleted;

        var subObjective1 = new MarkerEnterObjective(new Vector3(386.9004f, -89.74414f, 3.8843315f));
        var subObjective2 = new MarkerEnterObjective(new Vector3(393.80566f, -99.60156f, 5.2993988f));
        var objective1 = AddObjective(new OneOfObjective(subObjective1, subObjective2));
        objective1.Completed += ObjectiveCCompleted;

    }

    private void ObjectiveACompleted(Objective objective)
    {
        Entity.GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 1);
        Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Entered marker, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveBCompleted(Objective objective)
    {
        Entity.GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 2);
        Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Box delivered, objectives left: {Objectives.Count()}");
    }

    private void ObjectiveCCompleted(Objective objective)
    {
        Entity.GetRequiredComponent<JobStatisticsComponent>().AddPoints(1, 1);
        Entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Entered one of mater, objectives left: {Objectives.Count()}");
    }

}
