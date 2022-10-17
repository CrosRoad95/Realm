local function createLoginWindow(guiProvider)
	local window = guiProvider.window("Logowanie", 100, 100, 400, 400);
	local loginInput = guiProvider.input(100, 10, 200, 25, window);
	local passwordInput = guiProvider.input(100, 120, 200, 25, window);
	local loginButton = guiProvider.button("Zaloguj", 100, 300, 200, 20, window);

	guiProvider.setMasked(passwordInput, true)
	local form = createForm("login", {
		["login"] = loginInput,
		["password"] = passwordInput
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
