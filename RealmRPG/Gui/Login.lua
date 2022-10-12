local function createLoginWindow(guiProvider)
	local window = guiProvider.window("Logowanie", 100, 100, 400, 400);

	function stateChanged(key, value)
		iprint("payload", key, value)
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createLoginWindow, "login")
end)
setDebugViewActive(true)
