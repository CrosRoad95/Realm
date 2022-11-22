import * as TestModule from "Test/test.js"
import "panel.js"
import "discord.js"
import "login.js"
import "fractions.js"

Logger.information("Gameplay: currency={currency}", Gameplay.currency);
Logger.information("Localization: text={text}, try={text2}", Localization.translate("pl", "Test"), Localization.tryTranslate("pl", "doesn't exists", "test"));
Logger.information("startup.js, TestModule: {TestModule}", TestModule);
Logger.information("loaded modules: {modules}", JSON.stringify(getModules()));

const testVehicle = createVehicle(404, new Vector3(-10, -10, 3))
Logger.information("spawned vehicle: {testVehicle}", testVehicle);

const variantBlip = createVariantBlip(new Vector3(50, 50, 0));
const blip = createBlip(new Vector3(0, 0, 0), 6);
const pickup = createPickup(new Vector3(10, 10, 4), 1240);
const pickupCollision = createColSphere(new Vector3(10, 10, 4), 2);
pickupCollision.onEnter(e => {
    Logger.information("entered {element}", e)
})
Logger.information("spawned blip: {blip}, icon: {blipIcon}", blip, blip.icon);
const spawn = createSpawn(new Vector3(0, 20, 3));
//spawn.addRequiredPolicy("Admin");

const testRadarArea = createRadarArea(new Vector2(0, 0), new Vector2(50, 50), Color.fromArgb(255, 255, 0, 0));
const testVariantRadarArea = createVariantRadarArea(new Vector2(50, 0), new Vector2(50, 50));

Logger.information("createSpawn is persistant?: {persistant}", spawn.isPersistant());
addEventHandler("onPlayerJoin", async ({ player }) => {
    Logger.information("player joined: {player} isLoggedIn={isLoggedIn} language={language}", player.name, player.isLoggedIn, player.language);
    let account = await findAccountByUserName("Admin")
    Logger.information("account = {account}, is in use? {isInUse}", account, account.isInUse());
    player.debugView = true;
    player.debugWorld = true;
    //const loggedIn = await player.logIn(account, "asdASD123!@#");
    //if (!loggedIn)
    //    Logger.warning("Fail to log in");
    //Logger.information("is logged in?: {player} isLoggedIn={isLoggedIn}", player.name, player.isLoggedIn);
    //let account2 = await player.getAccount();
    //Logger.information("accounts ids: {a} = {b}", account.id, account2.id);
    //Logger.information("role: {a}", player.isInRole("admin"));
    //Logger.information("claims: {claims}", JSON.stringify(player.claims));
    //Logger.information("roles: {roles}", JSON.stringify(player.roles));

    player.openGui("login")
})

const spawns = getElementsByType("spawn");
Logger.information("spawns: {spawnsA} {spawnsB}", spawns.length, countElementsByType("spawn"));

for (var key in spawns) {
    Logger.information("Spawn: {spawnName}, is persistant: {isPersistant}", spawns[key].name, spawns[key].isPersistant());
}

const func = () => Logger.information("you should not see this");
addEventHandler("onPlayerJoin", func);
removeEventHandler("onPlayerJoin", func);

addEventHandler("onPlayerLogin", async ({ player, account }) => {
    Logger.information("player logged in: {player}, {account} is in use? {isInUse}", player, account, account.isInUse())
    await player.spawn(spawn);
    Logger.information("is player authorized to admin policy? {isAuthorized}", await player.account.authorizePolicy("Admin"))
    const playerAccount = player.account;
    {
        await playerAccount.setData("test", "sample value");
        let has = await playerAccount.hasData("test")
        let data = await playerAccount.getData("test");
        Logger.information("hasData={has} test='{data}'", has, data);
        let removed = await playerAccount.removeData("test");
        has = await playerAccount.hasData("test")
        Logger.information("removed={removed} hasData={has}", removed, has);
    }

    // License test
    {
        {
            let added = await playerAccount.addLicense("B")
            let licenses = await playerAccount.getAllLicenses()
            let hasLicense = await playerAccount.hasLicense("B")
            Logger.information("start hasLicense={hasLicense} added={added}, licenses={licenses}", hasLicense, added, licenses);
        }

        {
            await playerAccount.suspendLicense("B", 10, "odwalanie2")
            let isSuspended = await playerAccount.isLicenseSuspended("B")
            let licenses = await playerAccount.getAllLicenses()
            let hasLicense = await playerAccount.hasLicense("B")
            Logger.information("suspended: isSuspended={isSuspended} licenses={licenses} hasLicense={hasLicense}", isSuspended, licenses, hasLicense);
        }

        {
            await playerAccount.unSuspendLicense("B")
            let isSuspended = await playerAccount.isLicenseSuspended("B")
            let hasLicense = await playerAccount.hasLicense("B")
            let licenses = await playerAccount.getAllLicenses()
            Logger.information("unsuspended: isSuspended={isSuspended} licenses={licenses} hasLicenseB={hasLicense}", isSuspended, licenses, hasLicense);
        }

    }

    variantBlip.createFor(player, 5)
    //testVariantRadarArea.createFor(player, Color.fromArgb(255, 0, 255, 0));
})

addEventHandler("onPlayerLogout", ({ player }) => {
    Logger.information("logout {player}", player)
});

addEventHandler("onPlayerSpawn", ({ player, spawn }) => {
    Logger.information("player spawned: {player}", player)
});

//addEventHandler("onPlayerLogin", async ({ player, account }) => {
//    foo(); // Test exception
//});

addCommandHandler("foo", (player, args) => {
    Logger.information("command foo executed {player} {commandArguments}")
})

addCommandHandler("playtime", (player, args) => {
    if (player.isLoggedIn)
        Logger.information("playtime: current session: {0}, in total: {1}", player.account.currentSessionPlayTime, player.account.playTime)
})

addCommandHandler("money", (player, args) => {
    if (!player.isLoggedIn)
        return;

    player.account.money = player.account.money + 50;
})

addCommandHandler("foo2", (player, args) => {
    Logger.information("command foo2 executed {player} {commandArguments}")
}, ["admin"])

addCommandHandler("discordpolacz", (player, args) => {
    if (!player.isLoggedIn)
        return;

    if (player.account.isConnectedWithDiscordAccount()) {
        player.sendChatMessage(`Twoje konto jest już połączone z kontem discord.`);
        return;
    }

    if (player.account.hasPendingDiscordConnectionCode()) {
        player.sendChatMessage(`Masz już jeden kod wygenerowany, odczekaj chwile zanim spróbujesz ponownie.`);
        return;
    }

    const code = player.account.generateAndGetDiscordConnectionCode();
    player.sendChatMessage("Aby połączyć konto z serwera z kontem na discordzie podaj kod na kanele 'polacz' na discordzie:");
    player.sendChatMessage(`Twój kod ważny przez 2 minuty: '${code}'`);
    player.sendChatMessage("Kod został skopiowany do twojego schowka.");
    player.setClipboard(code);
})

addEventHandler("onPlayerDiscordConnected", async ({ player, discordUser }) => {
    player.sendChatMessage(`Pomyślnie połączyłeś konto na serwerze kontem na discordzie o id ${discordUser.id}`);

    await discordUser.sendTextMessage(`Dziękujemy za połączenie konta discord ${discordUser.username}!`);
});

addEventHandler("onDiscordUserChange", async ({ account, discord }) => {
    Logger.information("Konto discord: {discord} się zmieniło", discord.username)
    //await discord.sendTextMessage(`Twoje konto discord się zmieniło`);
});

addCommandHandler("discord", (player, args) => {
    if (!player.isLoggedIn)
        return;

    if (player.account.isConnectedWithDiscordAccount()) {
        player.sendChatMessage(`Twoje konto jest już połączone z kontem discord o id ${player.account.id}`);
    }
});

(async () => {
    try {
        await createPersistantVehicle("foo", 404, new Vector3(0,-5,3));
        let veh = await spawnPersistantVehicle("foo");
        veh.components.addComponent(new VehicleFuelComponent(2, 20, 4, 2.5, "petrol"));
        veh.isFrozen = false;
        const hasFuelComponent = veh.components.hasComponent(host.typeOf(VehicleFuelComponent));
        const fuelComp = veh.components.getComponent(host.typeOf(VehicleFuelComponent));
        Logger.information("fuelComp: {fuelComp} {amount}", fuelComp, fuelComp.amount);
        Logger.information("spawnPersistantVehicle: {veh} {components}, hasFuelComponent={hasFuelComponent}", veh, veh.components, hasFuelComponent);
    }
    catch (ex) {
        Logger.information("spawnPersistantVehicle ex: {exception}", ex.message);
    }
})()