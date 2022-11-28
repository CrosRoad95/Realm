const sapdFraction = getFractionById("fractionSapd")
Logger.information("sapdFraction: {sapdFraction}", sapdFraction);

addCommandHandler("startfractionsession", (player, args) => {
    sapdFraction.startSession(player)
})

addCommandHandler("sessionstat", (player, args) => {
    const session = player.getSession(host.typeOf(FractionSession));
    Logger.information("session: {session}, {code} = {elapsed}", typeof session, session.code, session.elapsed);
})
addEventHandler("onPlayerSessionStarted", ({ player, session }) => {
    Logger.information("onPlayerSessionStarted {player} {session}", player, session);
})

addEventHandler("onPlayerSessionStopped", ({ player, session }) => {
    Logger.information("onPlayerSessionStopped {player} {session}", player, session);
})