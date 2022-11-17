local guis = {};
local currentOpenedGui = nil;
local currentGuiProvider = nil;
local pendingFormsSubmissions = {}
local guiProviders = {}

function registerGuiProvider(gui, provider)
	guiProviders[gui] = provider
end

local function internalGetWindowHandleByName(name)
	if(guis[name] == nil)then
		error("Gui of name '"..tostring(name).."' doesn't' exists!")
	end
	if(guis[name].handle == false)then
		local handle, setStateCallback = guis[name].callback(currentGuiProvider);
		guis[name].handle = handle;
		guis[name].stateChanged = setStateCallback;
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
		handle = false,
	};
end

function openGui(name)
	if(currentOpenedGui)then
		return false;
	end

	currentOpenedGui = name;
	async(function()
		internalOpenGui(name)
	end)
	showCursor(true)
	return true;
end

function closeGui(name)
	if(not currentOpenedGui)then
		return false
	end
	
	internalCloseGui(name)
	showCursor(false)
	currentOpenedGui = nil;
end

addEvent("internalSubmitFormResponse", true)
addEventHandler("internalSubmitFormResponse", localPlayer, function(data)
	local id = table.remove(data, 1);
	local name = table.remove(data, 1);
	if(pendingFormsSubmissions[name])then
		coroutine.resume(pendingFormsSubmissions[name].coroutine, unpack(data))
		setTimer(function()
			pendingFormsSubmissions[name] = nil;
		end, 200, 1)
	else
		error("Form submission not found? a bug?")
	end
end)

function createForm(name, fields)
	return {
		submit = function()
			if(pendingFormsSubmissions[name])then
				return false
			end

			local data = {}
			for name,elementHandle in pairs(fields)do
				data[name] = currentGuiProvider.getValue(elementHandle)
			end

			local id = triggerServerEvent("internalSubmitForm", name, data);
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

local function internalCommonGuiProvider()
	return {
		closeCurrentGui = function()
			triggerServerEvent("internalRequestGuiClose", currentOpenedGui);
		end,
		navigateToGui = function(guiName)
			triggerServerEvent("internalNavigateToGui", guiName);
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
		end,
	}
end

local function entrypoint()
	currentGuiProvider = guiProviders["cegui"]
	if(currentGuiProvider == nil)then
		error("No gui provider found")
	end
	local internals = internalCommonGuiProvider()
	for name, func in pairs(internals)do
		currentGuiProvider[name] = func
	end

	addEvent("internalUiStatechanged", true)
	addEventHandler("internalUiStatechanged", localPlayer, function(data)
		local guiName, payloadKey, payloadValue = unpack(data);
		local currentGui = internalGetWindowHandleByName(guiName)
		if(currentGui)then
			currentGui.stateChanged(payloadKey, payloadValue);
		end
	end)

	addEvent("internalUiOpenGui", true)
	addEventHandler("internalUiOpenGui", localPlayer, function(guiName)
		openGui(guiName);
	end)
	
	addEvent("internalUiCloseGui", true)
	addEventHandler("internalUiCloseGui", localPlayer, function(guiName)
		closeGui(guiName);
	end)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	async(entrypoint)
end, true, "low");