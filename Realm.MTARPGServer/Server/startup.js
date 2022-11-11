import * as TestModule from "Test/test.js"
import "panel.js"
import "discord.js"
import "login.js"

Logger.information("startup.js, TestModule: {TestModule}", TestModule);
Logger.information("loaded modules: {modules}", JSON.stringify(getModules()));

const testVehicle = createVehicle("test", "perek", 404, new Vector3(-10, -10, 3))
Logger.information("spawned vehicle: {testVehicle}", testVehicle);
const spawn = createSpawn("dynamicSpawn", "test", new Vector3(0, 20, 3));
//spawn.addRequiredPolicy("Admin");

Logger.information("createSpawn is persistant?: {persistant}", spawn.isPersistant());
addEventHandler("onPlayerJoin", async ({ player }) => {
    Logger.information("player joined: {player} isLoggedIn={isLoggedIn}", player.name, player.isLoggedIn);
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

addEventHandler("onPlayerLogin", async ({player, account}) => {
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