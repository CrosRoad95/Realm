import * as TestModule from "Test/test.js"

Logger.information("startup.js, TestModule: {TestModule}", TestModule);

const spawn = createSpawn("dynamicSpawn", "test", new Vector3(0, 20, 3));
Logger.information("createSpawn is persistant?: {persistant}", spawn.isPersistant());
addEventHandler("onPlayerJoin", ({ player }) => {
    Logger.information("player joined: {player}", player.name);
    player.spawn(spawn);
})

const spawns = getElementsByType("spawn");
Logger.information("spawns: {spawnsA} {spawnsB}", spawns.length, countElementsByType("spawn"));

for (var key in spawns) {
    Logger.information("Spawn: {spawnName}, is persistant: {isPersistant}", spawns[key].name, spawns[key].isPersistant());
}

addEventHandler("onDiscordStatusChannelUpdate", context => {
    context.addLine(`======================================`);
    context.addLine(`Ostatnia aktualizacja statusu serwera: <t:${Math.floor(Date.now() / 1000)}:R>`);
})

addEventHandler("onDiscordStatusChannelUpdate", context => {
    const plrs = countElementsByType("player")
    context.addLine(`Ilość graczy: ${plrs}`);
})

const func = () => Logger.information("you should not see this");
addEventHandler("onPlayerJoin", func);
removeEventHandler("onPlayerJoin", func);

//Event.addHandler("onFormSubmit", context => {
//    Console.writeLine("event {0} {1}", context, context.name);
//})
