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
			local nameLabel = guiProvider.label(string.format("(%i) %s x%i", item.id, item.name, item.number), 10, y, 200, 24, scrollPane);
			guiProvider.setVerticalAlign(nameLabel, "center");
			if(bitAnd(item.actions, 1))then
				local useButton = guiProvider.button("Use", 235, y, 50, 24, scrollPane);
				guiProvider.onClick(useButton, function()
					guiProvider.invokeAction("doItemAction", {
						id = item.id,
						action = 1,
					})
				end)
			end
		end
	end
	
	window = guiProvider.window(string.format("Inventory %.2f/%.2f", state.Number, state.Size), 0,0, 300, 400);
	guiProvider.dockWindow(window, "rightCenter", "center")
	
  --local sampleLabel = guiProvider.label(inspect(state), 10, 20, 280, 25, window);
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
