addCommandHandler("playtime", (player, args) => {
  Logger.information("playtime: current session: {0}, in total: {1}", player.account.currentSessionPlayTime, player.account.playTime)
})

addCommandHandler("noclip", (player, args) => {
  player.noClip = !player.noClip
})

addCommandHandler("money", (player, args) => {
  player.account.money = player.account.money + 50;
})

addCommandHandler("gp", (player, args) => {
  Logger.information("{player} position: {position}", player, player.position);
})



