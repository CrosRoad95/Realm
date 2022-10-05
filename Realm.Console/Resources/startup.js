const spawnA = world.createSpawn("test", new vector3(0, 2, 3));
const spawnB = world.createSpawn("test", new vector3(0, 10, 3));

console.writeLine("Spawns ids: {0} {1}", spawnA, spawnB.id)
