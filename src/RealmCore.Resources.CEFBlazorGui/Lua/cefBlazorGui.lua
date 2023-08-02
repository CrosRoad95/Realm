local sx, sy = guiGetScreenSize();
local webBrowser = nil;
local browser = nil;
local selectedMode = "";
local currentPath = "index"

local function isValidPath(path)
	if(currentPath == "index" or currentPath == "")then
		return false;
	else
		return true;
	end
end

local function handleInvokeVoidAsync(identifier, args)
	if(identifier == "_guiReady")then
		if(isValidPath(currentPath))then
			internalSetVisible(true)
		end
	else
		triggerServerEvent("internalCEFInvokeVoidAsync", resourceRoot, identifier, args);
	end
end

local function handleInvokeAsync(identifier, promiseId, args)
	triggerServerEvent("internalCEFInvokeAsync", resourceRoot, identifier, promiseId, args);
end

local function handleRememberFrom(formName, promiseId, formData)
	local fileName = "@remember_"..formName..".json";
	if(fileExists(fileName))then
		fileDelete(fileName)
	end
	local file = fileCreate(fileName)
	fileWrite(file, formData)
	fileClose(file)
end

local function handleGetRememberFrom(formName, promiseId)
	local fileName = "@remember_"..formName..".json";
	if(not fileExists(fileName))then
		rejectPromise(promiseId)
		return;
	end
	local file = fileOpen(fileName, true)
	local content = fileRead(file, math.min(fileGetSize(file), 10000))
	fileClose(file)
	resolvePromise(promiseId, content);
end


local function handleForgetFrom(formName)
	local fileName = "@remember_"..formName..".json";
	if(fileExists(fileName))then
		fileDelete(fileName)
	end
end

function handleSetDevelopmentMode(enabled)
	setDevelopmentMode(true, enabled);
end

function handleToggleDevTools(enabled)
	toggleBrowserDevTools(webBrowser, enabled);
end

function internalSetVisible(visible)
	guiSetVisible(browser, visible);
	setBrowserRenderingPaused (webBrowser, not visible);
end

function handleSetVisible(visible)
	internalSetVisible(visible)
	showCursor(visible, false);
	if(not visible)then -- TODO: verify if it is okey.
		currentPath = "" 
	end
end

function handleSetPath(path, force, isAsync)
	currentPath = path;
	if(selectedMode == "dev")then
		loadBrowserURL(webBrowser, "http://localhost:5220/"..path)
	elseif(selectedMode == "prod")then
		local js = string.format("navigate(%q, %s)", path, force and "true" or "false");
		executeBrowserJavascript(webBrowser, js);
	end
	if(isAsync)then
		if(isValidPath(path))then
			showCursor(true, false);
		end
	end
end

local function handleLoad(mode, x, y)
	selectedMode = mode;
	if(mode == "dev")then
		browser = guiCreateBrowser( sx / 2 - x / 2, sy / 2 - y / 2, x, y, false, true, false)
		webBrowser = guiGetBrowser( browser )
		handleSetVisible(false)

		addEventHandler( "onClientBrowserCreated", webBrowser, 
			function( )
				local sourceBrowser = source;
				if(isBrowserDomainBlocked ( "localhost" ))then
					requestBrowserDomains({ "localhost" })
					local function handleClientBrowserWhitelistChange(newDomains)
						if newDomains[1] == "localhost" then
							triggerServerEvent("internalBrowserCreated", resourceRoot)
							--loadBrowserURL( sourceBrowser, "http://localhost:5220/" )
							removeEventHandler("onClientBrowserWhitelistChange", root, handleClientBrowserWhitelistChange);
						end
					end
					addEventHandler("onClientBrowserWhitelistChange", root, handleClientBrowserWhitelistChange);
				else
					triggerServerEvent("internalBrowserCreated", resourceRoot)
					--loadBrowserURL( sourceBrowser, "http://localhost:5220/" )
				end
			end
		)
	elseif(mode == "prod")then
		browser = guiCreateBrowser( sx / 2 - x / 2, sy / 2 - y / 2, x, y, true, true, false)
		webBrowser = guiGetBrowser( browser )
		handleSetVisible(false)
		setAjaxHandlers(webBrowser)
	
		addEventHandler( "onClientBrowserCreated", webBrowser, 
			function()
				if(fileExists("index.html"))then
					triggerServerEvent("internalBrowserCreated", resourceRoot)
					loadBrowserURL(source, "http://mta/local/index.html" )
					setDevelopmentMode(true, true);
					toggleBrowserDevTools(source, true);
				else
					loadBrowserURL(source, "http://mta/local/error.html" )
				end
			end
		)
	end
end

function resolvePromise(promiseId, data)
	executeBrowserJavascript(webBrowser, string.format("invokeAsyncSuccess(%i, %q)", promiseId, data));
end

function rejectPromise(promiseId, reason)
	executeBrowserJavascript(webBrowser, string.format("invokeAsyncError(%i, %q)", promiseId, reason));
end

function handleInvokeAsyncSuccess(promiseId, response)
	resolvePromise(promiseId, response)
end

function handleInvokeAsyncError(promiseId, reason)
	rejectPromise(promiseId, reason)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("Load", handleLoad)
	hubBind("SetDevelopmentMode", handleSetDevelopmentMode)
	hubBind("ToggleDevTools", handleToggleDevTools)
	hubBind("SetVisible", handleSetVisible)
	hubBind("SetPath", handleSetPath)
	hubBind("InvokeAsyncSuccess", handleInvokeAsyncSuccess)
	hubBind("InvokeAsyncError", handleInvokeAsyncError)
	
	addEvent("invokeVoidAsync")
	addEventHandler("invokeVoidAsync", resourceRoot, handleInvokeVoidAsync);
	addEvent("invokeAsync")
	addEventHandler("invokeAsync", resourceRoot, handleInvokeAsync);
	addEvent("rememberFrom")
	addEventHandler("rememberFrom", resourceRoot, handleRememberFrom);
	addEvent("getRememberFrom")
	addEventHandler("getRememberFrom", resourceRoot, handleGetRememberFrom);
	addEvent("forgetFrom")
	addEventHandler("forgetFrom", resourceRoot, handleForgetFrom);
end)
