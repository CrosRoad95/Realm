import * as TestModule from "Test/test.js"

Console.WriteLine("Module: {0}", TestModule.test)
const spawnA = World.CreateSpawn("test", new Vector3(0, 2, 3));
const spawnB = World.CreateSpawn("test", new Vector3(0, 10, 3));

Console.WriteLine("Spawns ids: {0} {1}", spawnA, spawnB.Id)

Event.AddHandler("onPlayerJoin", ({ Player }) => {
    Console.WriteLine("player joined: {0} {1}", String(Player), Player.Name);
    Player.Spawn(spawnA);
})

const func = () => Console.WriteLine("you should not see this");
Event.AddHandler("onPlayerJoin", func);
Event.RemoveHandler("onPlayerJoin", func);

Console.WriteLine("All spawns: {0}", World.GetElementsByType("spawn").length);

const plrs = World.GetElementsByType("player")
for (var key in plrs) {
    Console.WriteLine("Player: {0} = {1}", key, plrs[key].Name);
}

//try {
//    World.getElementsByType("unexisting type")
//}
//catch (ex) {
//    Console.writeLine("Exception caught: {0}", ex.toString());
//}