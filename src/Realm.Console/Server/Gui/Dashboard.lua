local function createDashboardWindow(guiProvider, state)
	local sampleLabel;

	local window = guiProvider.window("Dashboard window", 0, 0, 600, 400);
	guiProvider.centerWindow(window)
	local tabPanel = guiProvider.tabPanel (0, 0, 600, 400, window ) 
	do
		local tab = guiProvider.tab("General informations", tabPanel)
		sampleLabel = guiProvider.label(string.format("Money: %.2f", state.Money), 10, 30, 280, 25, tab);
		local incCounter = guiProvider.button("increase", 300, 60, 100, 20, tab);
		guiProvider.onClick(incCounter, function()
			guiProvider.invokeAction("counter")
		end)
	end
	do
		local tab = guiProvider.tab("Vehicles", tabPanel)
		local sampleLabel = guiProvider.label(inspect(state.VehicleLightInfos), 10, 20, 280, 25, tab);
		local grid,cellx, celly = utilities.createGrid(100, 100, 3, 3, 5);
		for i,v in ipairs(grid)do
			guiProvider.button(v[1].."x"..v[2], 20 + v[3], 50 + v[4], cellx, celly, tab);
		end
	end

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
