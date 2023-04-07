local function createShopWindow(guiProvider)
	local window = guiProvider.window("Sample shop", 0, 0, 300, 185);
	guiProvider.centerWindow(window)
	local information = guiProvider.label("hello shop", 10, 20, 280, 25, window);

	function stateChanged(key, value)
		iprint("payload", key, value)
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createShopWindow, "shop")
end)
