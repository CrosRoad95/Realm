addEvent("onLoggedIn", true)
setElementData(localPlayer, "translations", {}, false)
triggerServerEventWithId("sendLocalizationCode", getLocalization().code)
triggerServerEventWithId("sendScreenSize", guiGetScreenSize())

addEvent("updateTranslation", true)
addEventHandler("updateTranslation", localPlayer, function(translations)
	setElementData(localPlayer, "translations", translations[1], false)
end)

addEvent("internalSetClipboard", true)
addEventHandler("internalSetClipboard", localPlayer, function(content)
	setClipboard(content)
end)

addEvent("internalSetDevelopmentModeEnabled", true)
addEventHandler("internalSetDevelopmentModeEnabled", localPlayer, function(enabled)
	setDevelopmentMode(enabled)
end)