local function createLoginWindow(guiProvider)
	local window = guiProvider.window("Dashboard window", 0, 0, 600, 400);
	guiProvider.centerWindow(window)
	local information = guiProvider.label("sample dashboard", 10, 20, 280, 25, window);

	function stateChanged(key, value)
		iprint("payload", key, value)
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createLoginWindow, "dashboard")
end)
