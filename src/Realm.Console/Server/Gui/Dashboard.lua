local function createDashboardWindow(guiProvider, state)
	local sampleLabel;

	local window = guiProvider.window("Dashboard window", 0, 0, 600, 400);
	guiProvider.centerWindow(window)
	local tabPanel = guiProvider.tabPanel (0, 0, 600, 400, window ) 
	local tab1 = guiProvider.tab("General informations", tabPanel)
	do
		sampleLabel = guiProvider.label(inspect(state), 10, 20, 280, 25, tab1);
		local incCounter = guiProvider.button("increase", 300, 60, 100, 20, tab1);
		guiProvider.onClick(incCounter, function()
			guiProvider.invokeAction("counter")
		end)
	end
	guiProvider.tab("Vehicles", tabPanel)

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
