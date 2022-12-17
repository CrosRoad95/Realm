var veh = createVehicle(404, new Vector3(0,4,4))
Logger.information("new startup {veh}", veh)

addEventHandler("onPlayerJoin", ({entity}) => {
  var playercomp = entity.getComponent(host.typeOf(PlayerElementCompoent));
  playercomp.spawn(new Vector3(0,0,4))
})