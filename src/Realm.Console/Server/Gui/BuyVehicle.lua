local function createLoginWindow(guiProvider, state)
	local window = guiProvider.window("Sample buy vehicle", 0, 0, 300, 185);
	guiProvider.centerWindow(window)
	guiProvider.label(string.format("Vehicle: %s\nPrice: $%.2f", state.Name, state.Price), 10, 20, 280, 60, window);

	local form = createForm("buy", { });

	local buyButton = guiProvider.button("Buy", 20, 150, 260, 20, window);

	guiProvider.onClick(buyButton, function()
		local success, data = form.submit()
	end)

	function stateChanged(key, value)
		iprint("payload", key, value)
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createLoginWindow, "buyVehicle")
end)
