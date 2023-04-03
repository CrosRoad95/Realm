local function createGui(guiProvider, state)

    function handleLogin()
        local success, data = getFormByName("login").submit()
		if(success)then
			if(guiProvider.getSelected(rememberPassword))then
				guiProvider.rememberForm(form)
			else
				guiProvider.forgetForm(form)
			end
		else
			guiProvider.setValue(information, data)
		end
    end

    function handleNavigateToRegister()
        guiProvider.invokeAction("navigateToRegister")
    end


local window = guiProvider.window("Logowanie", 0, 0, 300, 185);
guiProvider.centerWindow(window);

    local information = guiProvider.label("", 10, 20, 280, 25, window);
    local _ = guiProvider.label("Login:", 10, 45, 90, 25, window);
    local _ = guiProvider.label("Hasło:", 10, 80, 90, 25, window);
    
        local input1 = guiProvider.input(100, 45, 190, 25, window);
        local input2 = guiProvider.input(100, 80, 190, 25, window);
        local button1 = guiProvider.button("Zaloguj", 130, 80, 160, 25, window);

    local login = createForm("login", {
	["login"] = input1,
	["password"] = input2,
});
    	local rememberPassword = guiProvider.checkbox("Zapamiętaj", 10, 120, 100, 20, false, window);

    local navigateToRegister = guiProvider.button("Nie mam konta", 10, 80, 100, 25, window);
guiProvider.onClick(navigateToRegister, handleNavigateToRegister);


end

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGui(createGui, "login")
end)
