local guis = {};
local guiRefs = 0
local currentOpenedGui = {};
local currentGuiProvider = nil;
local pendingFormsSubmissions = {}
local guiProviders = {}
local currentGui = nil
local lastCreatedWindow = nil
local cooldown = {}

function getCurrentGuiName()
	return currentGui;
end

function registerGuiProvider(gui, provider)
	guiProviders[gui] = provider
end

local function createErrorGui(guiProvider, err, guiName)
	local window = guiProvider.window("Błąd w gui: "..tostring(guiName), 0, 0, 500, 200);
	guiProvider.centerWindow(window)
	local errorLabel = guiProvider.label(err, 20, 30, 260, 160, window);
	guiProvider.setHorizontalAlign(errorLabel, "left", true)
	return window
end

local function internalGetWindowHandleByName(name)
	if(guis[name] == nil)then
		error("Gui of name '"..tostring(name).."' doesn't' exists!")
	end
	if(guis[name].handle == false)then
		currentGui = name;
		currentGuiProvider.currentGui = currentGui;
		local status, retval = pcall(function()
			local windowHandle, setStateCallback = guis[name].callback(currentGuiProvider);
			guis[name].handle = windowHandle;
			guis[name].stateChanged = setStateCallback;
			return true
		end)
		if(not status)then
			if(lastCreatedWindow ~= nil)then
				currentGuiProvider.close(lastCreatedWindow)
				lastCreatedWindow = nil
			end
			local windowHandle = createErrorGui(currentGuiProvider, retval, name)
			guis[name].handle = windowHandle;
			guis[name].stateChanged = function()end
		end
		currentGui = nil;
	end

	return guis[name];
end

local function internalOpenGui(name)
	waitForTranslations();
	local result = currentGuiProvider.open(internalGetWindowHandleByName(name).handle)
	if(result ~= true)then
		error("Failed to open gui '"..tostring(name).."'");
	end
end

local function internalCloseGui(name)
	local result = currentGuiProvider.close(internalGetWindowHandleByName(name).handle)
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
		handle = false,
	};
end

local currentOpenedGui = {}
function openGui(name, cursorless)
	if(currentOpenedGui[name])then
		error("Gui: "..tostring(name).." already opened");
	end
	currentOpenedGui[name] = true
	if(not cursorless)then
		guiRefs = guiRefs + 1
	end
	async(function()
		internalOpenGui(name)
	end)
	if(guiRefs > 0)then
		showCursor(true)
	end
	return true;
end

function closeGui(name)
	if(not currentOpenedGui[name])then
		return false
	end
	
	internalCloseGui(name)
	currentOpenedGui[name] = nil;
	guiRefs = guiRefs - 1
	if(guiRefs == 0)then
		showCursor(false)
	end
end

addEvent("internalSubmitFormResponse", true)
addEventHandler("internalSubmitFormResponse", localPlayer, function(id, name, data)
	if(pendingFormsSubmissions[name])then
		local success, result = unpack(data)
		coroutine.resume(pendingFormsSubmissions[name].coroutine, success, unpack(result))
		setTimer(function()
			pendingFormsSubmissions[name] = nil;
		end, 200, 1)
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
			local fileName = "@remember_"..name..".json";
			if(fileExists(fileName))then
				fileDelete(fileName)
			end
		end
	}
end

local function entrypoint()
	currentGuiProvider = guiProviders["dgs"]
	if(currentGuiProvider == nil)then
		error("No gui provider found")
	end
	local internals = internalCommonGuiProvider()
	for name, func in pairs(internals)do
		currentGuiProvider[name] = func
	end

	local _onClick = currentGuiProvider.onClick;
	local _window = currentGuiProvider.window;
	currentGuiProvider.window = function(...)
		lastCreatedWindow = _window(...)
		return lastCreatedWindow
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

	addEvent("internalUiStatechanged", true)
	addEventHandler("internalUiStatechanged", localPlayer, function(data)
		local guiName, payloadKey, payloadValue = unpack(data);
		local currentGuiHandle = internalGetWindowHandleByName(guiName)
		if(currentGuiHandle)then
			currentGuiHandle.stateChanged(payloadKey, payloadValue);
		end
	end)

	addEvent("internalUiOpenGui", true)
	addEventHandler("internalUiOpenGui", localPlayer, function(guiName, cursorless)
		openGui(guiName, cursorless);
	end)
	
	addEvent("internalUiCloseGui", true)
	addEventHandler("internalUiCloseGui", localPlayer, function(guiName)
		closeGui(guiName);
	end)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	async(entrypoint)
end, true, "low");