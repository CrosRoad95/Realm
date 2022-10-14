local guis = {};
local currentOpenedGui = nil;
local currentGuiProvider = nil;
local pendingFormsSubmissions = {}

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
	internalOpenGui(name)
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

function createForm(name, fields)
	return {
		submit = function()
			if(pendingFormsSubmissions[name])then
				return false
			end
			pendingFormsSubmissions[name] = true;
			local data = {}
			for name,v in pairs(fields)do
				data[name] = currentGuiProvider.getValue(v)
			end
			print("trigger");
			triggerServerEvent("internalSubmitForm", resourceRoot, name, data);
			return true;
		end,
	};
end

local function entrypoint()
	currentGuiProvider = getCeguiUIProvider();
	
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
addEventHandler("onClientResourceStart", resourceRoot, entrypoint,  true, "low");