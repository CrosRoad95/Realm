local guis = {};
local guiRefs = 0
local currentOpenedGui = {};
local currentGuiProvider = nil;
local pendingFormsSubmissions = {}
local guiProviders = {}
local currentGui = nil
local lastCreatedWindow = nil
local cooldown = {}
local currentOpenedGui = {}
local debugToolsEnabledState = false;
local screenX, screenY = guiGetScreenSize();
local sx,sy = screenX, screenY
local originalScreenX, originalScreenY = screenX, screenY;
local dockingAreasRendering = false
local dockingAreas = {
	["topLeft"] = {0, 0, 500, 400},
	["topCenter"] = {sx / 4, 0, sx / 2, 300},
	["topRight"] = {sx - 500, 0, 500, 400},
	["leftCenter"] = {50,sy / 2 - 300,300,600},
	["center"] = {sx / 2 - 400, sy / 2 - 300, 800, 600},
	["rightCenter"] = {sx - 300 - 50, sy / 2 - 300,300,600},
	["leftBottom"] = {0, sy - 400, 500, 400},
	["centerBottom"] = {sx / 4, sy - 300, sx / 2, 300},
	["rightBottom"] = {sx - 500, sy - 400, 500, 400},
}
setDebugViewActive(true)
addEvent("guiElementCreated")

function guiGetScreenSize()
	return screenX, screenY;
end

function getCurrentGuiName()
	return currentGui;
end

function registerGuiProvider(gui, provider)
	guiProviders[gui] = provider
end

local function createErrorGui(guiProvider, err, guiName)
	local window = guiProvider.window("Error in gui: "..tostring(guiName), 0, 0, 500, 200);
	guiProvider.centerWindow(window)
	local errorLabel = guiProvider.label(err, 20, 30, 260, 160, window);
	guiProvider.setHorizontalAlign(errorLabel, "left", true)
	return window
end

local function internalGetWindowHandleByName(name, defaultState)
	if(guis[name] == nil)then
		local availiableGuis = {}
		for name in pairs(guis)do
			availiableGuis[#availiableGuis + 1] = "'"..name.."'"
		end
		error("Gui of name '"..tostring(name).."' doesn't' exists. Availiable guis: "..table.concat(availiableGuis, ", "))
	end
	local stateMd5 = nil;
	if(defaultState ~= nil)then
		stateMd5 = md5(inspect(defaultState));

		if(guis[name].handle and guis[name].stateMd5 ~= stateMd5)then
			currentGuiProvider.destroy(guis[name].handle)
			guis[name].handle = false;
			guis[name].stateMd5 = "";
			guis[name].state = defaultState
		end
	end

	if(guis[name].handle == false)then
		currentGui = name;
		currentGuiProvider.currentGui = currentGui;
		local success, errorMessage = pcall(function()
			guis[name].state = defaultState
			guis[name].stateCallbacks = {}
			local windowHandle, setStateCallback = guis[name].callback(currentGuiProvider, defaultState);
			guis[name].handle = windowHandle;
			guis[name].stateChanged = setStateCallback or function()end;
			return true
		end)
		if(not success)then
			if(lastCreatedWindow ~= nil)then
				currentGuiProvider.close(lastCreatedWindow)
				lastCreatedWindow = nil
			end
			local windowHandle = createErrorGui(currentGuiProvider, errorMessage, name)
			guis[name].handle = windowHandle;
			guis[name].stateCallbacks = {}
			guis[name].stateChanged = function()end
		end
		guis[name].stateMd5 = stateMd5;
		currentGui = nil;
	end
	return guis[name];
end

local function internalOpenGui(name, defaultState)
	waitForTranslations();
	local result = currentGuiProvider.open(internalGetWindowHandleByName(name, defaultState).handle)
	if(result ~= true)then
		error("Failed to open gui '"..tostring(name).."'");
	end
end

local function internalCloseGui(name)
	local result = currentGuiProvider.close(internalGetWindowHandleByName(name, nil).handle)
	if(result ~= true)then
		error("Failed to close gui '"..tostring(name).."'");
	end
end

function registerGui(callback, name)
	if(guis[name])then
		error("Gui of name '"..tostring(name).."' already exists!")
	end
	if(type(callback) ~= "function")then	
		error("Callback is not a function")
	end
	guis[name] = {
		callback = callback,
		name = name,
		stateMd5 = "",
		handle = false,
	};
end

function openGui(name, cursorless, defaultState)
	if(currentOpenedGui[name])then
		error("Gui: "..tostring(name).." already opened");
	end
	currentOpenedGui[name] = true
	if(not cursorless)then
		guiRefs = guiRefs + 1
	end
	async(function()
		internalOpenGui(name, defaultState)
	end)
	if(guiRefs > 0)then
		showCursor(true, false)
	end
	return true;
end

function closeGui(name, cursorless)
	if(not currentOpenedGui[name])then
		return false
	end
	
	internalCloseGui(name)
	currentOpenedGui[name] = nil;
	if(not cursorless)then
		guiRefs = guiRefs - 1
		if(guiRefs <= 0)then
			showCursor(false)
			guiRefs = 0
		end
	end
end

function renderDockingAreas()
	for k,v in pairs(dockingAreas)do
		dxDrawRectangle(v[1], v[2], v[3], v[4], tocolor(255,0,0,50))
		dxDrawText(string.format("%s - %ix%i", k, v[3], v[4]), v[1], v[2], v[1] + v[3], v[2] + v[4])
	end
end

function setRenderDockingAreasEnabled(enabled)
	dockingAreasRendering = not dockingAreasRendering
	if(dockingAreasRendering)then
		addEventHandler("onClientRender", root, renderDockingAreas)
	else
		removeEventHandler("onClientRender", root, renderDockingAreas)
	end
end

function toggleRenderDockingAreasEnabled()
	setRenderDockingAreasEnabled(not dockingAreasRendering)
end

function enableDebugTools()
	bindKey("o", "down", toggleRenderDockingAreasEnabled)
end

function disableDebugTools()
	setRenderDockingAreasEnabled(false)
	unbindKey("o", "down", toggleRenderDockingAreasEnabled)
end

addEvent("internalSubmitFormResponse", true)
addEventHandler("internalSubmitFormResponse", localPlayer, function(id, name, data)
	if(pendingFormsSubmissions[name])then
		if(coroutine.status(pendingFormsSubmissions[name].coroutine) == "suspended")then
			local success, result = unpack(data)
			coroutine.resume(pendingFormsSubmissions[name].coroutine, success, unpack(result))
			setTimer(function()
				pendingFormsSubmissions[name] = nil;
			end, 200, 1)
		else
			error("Failed to response to form "..tostring(name)..". Probably already responded.");
		end
	else
		error("Form submission not found? a bug?")
	end
end)

function createForm(name, fields)
	if(currentGui == nil)then
		error("Can't use createFrom outside gui constructor.'")
	end
	local _currentGuiName = currentGui;
	return {
		submit = function()
			if(pendingFormsSubmissions[name])then
				return false
			end

			local data = {}
			for name,elementHandle in pairs(fields)do
				data[name] = currentGuiProvider.getValue(elementHandle)
			end

			local id = triggerServerEventWithId("internalSubmitForm", _currentGuiName, name, data);
			pendingFormsSubmissions[name] = {
				id = id,
				coroutine = coroutine.running()
			};
			return coroutine.yield();
		end,
		getFields = function()
			return fields;
		end,
		getName = function()
			return name;
		end,
	};
end

function cooldownCheck(key, time)
  if( cooldown[key] and cooldown[key] > getTickCount())then
    return false
  else
    cooldown[key] = getTickCount() + time
    return true
  end
end

local function internalCommonGuiProvider()
	local _currentGuiName = currentGui;
	return {
		invokeAction = function(actionName, data)
			if(cooldownCheck(actionName, 600))then
				triggerServerEventWithId("internalActionExecuted", getCurrentGuiName(), actionName, data);
			end
		end,
		tryLoadRememberedForm = function(form)
			local name = form.getName()
			local fileName = "@remember_"..name..".json";
			if(not fileExists(fileName))then
				return false;
			end
			local file = fileOpen(fileName)
			local content = fileRead(file, fileGetSize(file))
			fileClose(file)
			local data = fromJSON(content)
			if(not data or not type(data) == "table")then
				return false;
			end
			
			local fields = form.getFields()
			for name, field in pairs(data)do
				if(fields[name])then
					currentGuiProvider.setValue(fields[name], field.value)
				end
			end
			return true;
		end,
		rememberForm = function(form)
			local name = form.getName()
			local data = {}
			for name,elementHandle in pairs(form.getFields())do
				local value = currentGuiProvider.getValue(elementHandle)
				if(string.len(toJSON(value)) > 1000)then
					return false
				end
				data[name] = {
					value = value,
				}
			end
			local fileName = "@remember_"..name..".json";
			if(fileExists(fileName))then
				fileDelete(fileName)
			end
			local file = fileCreate(fileName)
			fileWrite(file, toJSON(data))
			fileClose(file)
		end,
		forgetForm = function(form)
			local name = form.getName()
			local fileName = "@remember_"..name..".json";
			if(fileExists(fileName))then
				fileDelete(fileName)
			end
		end,
		createCenteredWindow = function(title, sx, sy)
			local window = currentGuiProvider.window(title, 0, 0, sx, sy);
			currentGuiProvider.centerWindow(window)
			return window;
		end,
		dockWindow = function(windowHandle, dockName, positionName)
			if(dockingAreas[dockName])then
				local dock = dockingAreas[dockName]
				local wx,wy = currentGuiProvider.getSize(windowHandle)
				local dockCenterX, dockCenterY = dock[1] + dock[3] / 2, dock[2] + dock[4] / 2;
				if(positionName == "center")then
					currentGuiProvider.setWindowPosition(windowHandle, dockCenterX - wx / 2, dockCenterY - wy / 2)
				else
					error("Docking area position: '"..tostring(positionName).."' is not supported")
				end
			else
				error("Docking area: '"..tostring(dockName).."' not found")
			end
		end,
	}
end

addEventHandler("guiElementCreated", resourceRoot, function(...)
	--iprint("guiElementCreated",...)
end)

local function entrypoint()
	if(selectedGuiProvider == nil)then
		error("No gui provider selected.")
	end
	currentGuiProvider = guiProviders[selectedGuiProvider]
	if(currentGuiProvider == nil)then
		error("No gui provider found")
	end
	local internals = internalCommonGuiProvider()
	for name, func in pairs(internals)do
		currentGuiProvider[name] = func
	end

	local _onClick = currentGuiProvider.onClick;
	local _window = currentGuiProvider.window;
	local _createInput = currentGuiProvider.input;
	local _createButton = currentGuiProvider.button;
	local _createLabel = currentGuiProvider.label;
	local _createCheckbox = currentGuiProvider.checkbox;
	local _createScrollPane = currentGuiProvider.scrollPane;
	local _createTabPanel = currentGuiProvider.tabPanel;
	local _createTab = currentGuiProvider.tab;

	currentGuiProvider.input = function(...)
		local input = _createInput(...)
		triggerEvent("guiElementCreated", resourceRoot, "input", input)
		return input
	end

	currentGuiProvider.button = function(...)
		local button = _createButton(...)
		triggerEvent("guiElementCreated", resourceRoot, "button", button)
		return button
	end
	
	currentGuiProvider.label = function(...)
		local label = _createLabel(...)
		triggerEvent("guiElementCreated", resourceRoot, "label", label)
		return label
	end
	
	currentGuiProvider.checkbox = function(...)
		local checkbox = _createCheckbox(...)
		triggerEvent("guiElementCreated", resourceRoot, "checkbox", checkbox)
		return checkbox
	end
	
	currentGuiProvider.scrollPane = function(...)
		local scrollPane = _createScrollPane(...)
		triggerEvent("guiElementCreated", resourceRoot, "scrollPane", scrollPane)
		return scrollPane
	end
	
	currentGuiProvider.tabPanel = function(...)
		local tabPanel = _createTabPanel(...)
		triggerEvent("guiElementCreated", resourceRoot, "tabPanel", tabPanel)
		return tabPanel
	end
	
	currentGuiProvider.tab = function(...)
		local tab = _createTab(...)
		triggerEvent("guiElementCreated", resourceRoot, "tab", tab)
		return tab
	end
	
	currentGuiProvider.window = function(...)
		lastCreatedWindow = _window(...)
		return lastCreatedWindow
	end

	currentGuiProvider.usingState = function(stateKey, callback)
		if(currentGui == nil)then
			error("Can't use usingState outside gui constructor.'")
		end
		
		local _currentGui = getCurrentGuiName();
		local gui = guis[_currentGui];
		local createdElements = {}
		local function handleElementCreated(elementType, element)
			createdElements[#createdElements + 1] = element;
		end
		local updateCallback = function(newState)
			for i,v in ipairs(createdElements)do
				currentGuiProvider.destroy(v)
			end
			createdElements = {}
			addEventHandler("guiElementCreated", resourceRoot, handleElementCreated)
			callback(gui.state[stateKey])
			removeEventHandler("guiElementCreated", resourceRoot, handleElementCreated)
		end
		gui.stateCallbacks[stateKey] = updateCallback;
		updateCallback(gui.state[stateKey])
	end

	currentGuiProvider.onClick = function(elementHandle, callback)
		if(currentGui == nil)then
			error("Can't use onClick outside gui constructor.'")
		end
		local _currentGui = getCurrentGuiName();
		_onClick(elementHandle, function(...)
			currentGui = _currentGui;
			callback(...)
			currentGui = nil;
		end)
	end

	addEvent("internalUiStateChanged", true)
	addEventHandler("internalUiStateChanged", localPlayer, function(guiName, changes)
		local currentGuiHandle = internalGetWindowHandleByName(guiName)
		if(currentGuiHandle)then
			currentGui = guiName;
			currentGuiHandle.state = changes
			for k,v in pairs(changes)do
				if(currentGuiHandle.stateCallbacks[k])then
					currentGuiHandle.stateCallbacks[k](v)
				end
				currentGuiHandle.stateChanged(k, v);
			end
			currentGui = nil;
		end
	end)

	addEvent("internalUiOpenGui", true)
	addEventHandler("internalUiOpenGui", localPlayer, function(guiName, cursorless, defaultState)
		outputConsole("internalUiOpenGui")
		openGui(guiName, cursorless, defaultState);
	end)
	
	addEvent("internalUiCloseGui", true)
	addEventHandler("internalUiCloseGui", localPlayer, function(guiName, cursorless)
		closeGui(guiName, cursorless);
	end)
	
	addEvent("internalUiDebugToolsEnabled", true)
	addEventHandler("internalUiDebugToolsEnabled", localPlayer, function(enabled)
		if(debugToolsEnabledState ~= enabled)then
			if(enabled)then
				enableDebugTools()
			else
				disableDebugTools()
			end
			debugToolsEnabledState = enabled
		end
	end)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	async(entrypoint)
end, true, "low");
