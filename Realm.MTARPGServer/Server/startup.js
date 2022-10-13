import * as TestModule from "Test/test.js"

Console.writeLine("Module: {0}", TestModule.test)
const spawnA = World.createSpawn("dynamicSpawn", "test", new Vector3(0, 20, 3));

Console.writeLine("Spawns ids: {0} {1}", spawnA.id, spawnA.isPersistant)

const provisionedSpawn = World.getElementByTypeAndId("spawn", "spawnA");
Console.writeLine("Provisioned spawn id: '{0}', persistant={1}", provisionedSpawn.id, provisionedSpawn.isPersistant);

Event.addHandler("onPlayerJoin", ({ player }) => {
    Console.writeLine("player joined: {0} {1}", String(player), player.name);
    player.spawn(spawnA);
})

const func = () => Console.writeLine("you should not see this");
Event.addHandler("onPlayerJoin", func);
Event.removeHandler("onPlayerJoin", func);

const spawns = World.getElementsByType("spawn");
Console.writeLine("All spawns: {0}", spawns.length);
for (var key in spawns) {
    Console.writeLine("Spawn: {0} = {1}, persistant: {2}", key, spawns[key].name, spawns[key].isPersistant);
}

const plrs = World.getElementsByType("player")
Console.writeLine("All players: {0}", plrs.length);
for (var key in plrs) {
    Console.writeLine("Player: {0} = {1}", key, plrs[key].name);
}

//try {
//    World.getElementsByType("unexisting type")
//}
//catch (ex) {
//    Console.writeLine("Exception caught: {0}", ex.toString());
//}