local function createLoginWindow(guiProvider)
	local window = guiProvider.window("Logowanie", 100, 100, 400, 400);
	local loginInput = guiProvider.input(10, 10, 200, 20, window);
	local loginButton = guiProvider.button("Zaloguj", 10, 200, 200, 20, window);

	local form = createForm("test123", {
		["test"] = loginInput
	});

	guiProvider.onClick(loginButton, function()
		local success, data = form.submit()
		iprint("response", success, data)
	end)

	function stateChanged(key, value)
		iprint("payload", key, value)
	end
	return window, stateChanged
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createLoginWindow, "login")
end)
setDebugViewActive(true)
