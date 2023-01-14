local function createInventoryWindow(guiProvider, state)
	local function createTitle()
		return string.format("Inventory %.2f/%.2f", state.Number, state.Size)
	end
	local scrollPane;
	local window;
	local function createItems()
		if(scrollPane ~= nil)then
			guiProvider.destroy(scrollPane)
			scrollPane = nil;
		end
		scrollPane = guiProvider.scrollPane(0,0, 300, 375, window)
		for i,item in ipairs(state.Items)do
			local y = 5 + i * 25
			local nameLabel = guiProvider.label(string.format("(%i) %s x%i", item.id, item.name, item.number), 10, y, 200, 25, scrollPane);
			guiProvider.setVerticalAlign(nameLabel, "center");
			local useButton = guiProvider.button("Use", 235, y, 50, 25, scrollPane);
			guiProvider.onClick(useButton, function()
				guiProvider.invokeAction("use", {
					id = item.id,
				})
			end)
		end
	end

	window = guiProvider.window(string.format("Inventory %.2f/%.2f", state.Number, state.Size), 0,0, 300, 400);
	guiProvider.dockWindow(window, "rightCenter", "center")

	createItems();

	function stateChanged(key, value)
		state[key] = value;
		if(key == "Items")then
			createItems();
		elseif(key == "Number")then
			guiProvider.setValue(window, createTitle())
		end
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createInventoryWindow, "inventory")
end)
