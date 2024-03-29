﻿local function createLoginWindow(guiProvider, state)
	local window = guiProvider.window("Sample window", 0, 0, 300, 185);
	guiProvider.centerWindow(window)
	local information = guiProvider.label(inspect(state), 10, 20, 280, 25, window);

	function stateChanged(key, value)
		iprint("payload", key, value)
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createLoginWindow, "test")
end)
