const spawnA = World.createSpawn("test", new Vector3(0, 2, 3));
const spawnB = World.createSpawn("test", new Vector3(0, 10, 3));

Console.writeLine("Spawns ids: {0} {1}", spawnA, spawnB.id)

Event.addHandler("onPlayerJoin", ({player}) => {
    Console.writeLine("player joined: {0}", player.name);
})
