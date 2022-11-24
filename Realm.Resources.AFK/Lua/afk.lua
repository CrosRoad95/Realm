function onClientMTAFocusChange(windowFocused)
	if(windowFocused)then
		triggerServerEvent("internalAFKStop", resourceRoot)
	else
		triggerServerEvent("internalAFKStart", resourceRoot)
	end
end
addEventHandler("onClientMTAFocusChange", root, onClientMTAFocusChange)