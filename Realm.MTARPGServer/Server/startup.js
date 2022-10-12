import * as TestModule from "Test/test.js"

Console.writeLine("Module: {0}", TestModule.test)
const spawnA = World.createSpawn("test", new Vector3(0, 2, 3));
const spawnB = World.createSpawn("test", new Vector3(0, 10, 3));

Console.WriteLine("Spawns ids: {0} {1}", spawnA, spawnB.id)

Event.addHandler("onPlayerJoin", ({ player }) => {
    Console.writeLine("player joined: {0} {1}", String(Player), Player.mame);
    Player.spawn(spawnA);
})

const func = () => Console.writeLine("you should not see this");
Event.addHandler("onPlayerJoin", func);
Event.removeHandler("onPlayerJoin", func);

Console.writeLine("All spawns: {0}", World.getElementsByType("spawn").length);

const plrs = World.getElementsByType("player")
for (var key in plrs) {
    Console.writeLine("Player: {0} = {1}", key, plrs[key].name);
}

//try {
//    World.getElementsByType("unexisting type")
//}
//catch (ex) {
//    Console.writeLine("Exception caught: {0}", ex.toString());
//}