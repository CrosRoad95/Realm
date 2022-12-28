local state = {}
local function createDashboardWindow(guiProvider, defaultState)
	state = defaultState
	local window = guiProvider.window("Dashboard window", 0, 0, 600, 400);
	guiProvider.centerWindow(window)
	local sampleLabel = guiProvider.label(inspect(state), 10, 20, 280, 25, window);

	local incCounter = guiProvider.button("increase", 300, 60, 100, 20, window);
	guiProvider.onClick(incCounter, function()
		guiProvider.invokeAction("counter")
	end)

	function stateChanged(key, value)
		state[key] = value;
		if(key == "Counter")then
			guiProvider.setValue(sampleLabel, inspect(state))
		end
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createDashboardWindow, "dashboard")
end)
