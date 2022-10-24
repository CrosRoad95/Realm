addEventHandler("onPlayerJoin", ({ player }) => {
    webPanelAddSnackbar(`${player.name} wszedł na serwer`);
})