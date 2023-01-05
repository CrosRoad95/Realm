local state = {}
local function createInventoryWindow(guiProvider, defaultState)
	state = defaultState
	local window = guiProvider.window("Inventory", 0,0, 300, 400);
	guiProvider.dockWindow(window, "rightCenter", "center")
	local sampleLabel = guiProvider.label(inspect(state), 10, 20, 280, 25, window);

	function stateChanged(key, value)

	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createInventoryWindow, "inventory")
end)
