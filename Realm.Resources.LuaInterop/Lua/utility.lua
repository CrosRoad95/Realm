setElementData(localPlayer, "translations", {}, false)
triggerServerEvent("sendLocalizationCode", getLocalization().code)

addEvent("updateTranslation", true)
addEventHandler("updateTranslation", localPlayer, function(translations)
	setElementData(localPlayer, "translations", translations[1], false)
end)