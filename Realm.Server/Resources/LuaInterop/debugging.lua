addEvent("internalSetDebuggingEnabled", true)
addEventHandler("internalSetDebuggingEnabled", localPlayer, setDebugViewActive)

addEventHandler("onClientDebugMessage", root, function(message, level, file, line)
	triggerServerEvent("internalDebugMessage", message, level, file, line)
end)
