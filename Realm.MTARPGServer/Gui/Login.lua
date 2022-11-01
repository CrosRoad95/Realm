local function createLoginWindow(guiProvider)
	local window = guiProvider.window("Logowanie", 0, 0, 300, 160);
	guiProvider.centerWindow(window)
	local information = guiProvider.label("", 10, 20, 280, 25, window);
	guiProvider.label("Login:", 10, 45, 90, 25, window);
	local loginInput = guiProvider.input(100, 45, 180, 25, window);
	guiProvider.label("Hasło:", 10, 80, 90, 25, window);
	local passwordInput = guiProvider.input(100, 80, 180, 25, window);
	guiProvider.button("Nie mam konta", 10, 120, 100, 20, window);
	local loginButton = guiProvider.button("Zaloguj", 120, 120, 180, 20, window);

	guiProvider.setMasked(passwordInput, true)
	local form = createForm("login", {
		["login"] = loginInput,
		["password"] = passwordInput
	});
	guiProvider.tryLoadRememberedForm(form);

	guiProvider.onClick(loginButton, function()
		guiProvider.rememberForm(form)
		local success, data = form.submit()
		iprint("response", success, data)
		if(success)then
		else
			guiProvider.setValue(information, data)
		end
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
