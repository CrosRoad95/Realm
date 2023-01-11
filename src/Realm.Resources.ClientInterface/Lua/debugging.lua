addEvent("internalSetWorldDebuggingEnabled", true)
addEventHandler("internalSetWorldDebuggingEnabled", localPlayer, function(enabled)
	setElementData(localPlayer, "_worldDebugging", enabled, false)
end)

addEventHandler("onClientDebugMessage", root, function(message, level, file, line)
	triggerServerEventWithId("internalDebugMessage", message, level, file, line)
end)
