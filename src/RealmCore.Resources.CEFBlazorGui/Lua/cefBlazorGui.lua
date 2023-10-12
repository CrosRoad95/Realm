local sx, sy = guiGetScreenSize();
local webBrowser = nil;
local browser = nil;
local selectedMode = "";
local currentPath = "index"
local isRemote = false;
local trace = true;

local function itrace(...)
	if(trace)then
		iprint(getTickCount(), ...)
	end
end

local function isValidPath(path)
	if(currentPath == "index" or currentPath == "")then
		return false;
	else
		return true;
	end
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

function handleToggleDevTools(enabled)
	toggleBrowserDevTools(webBrowser, enabled);
end

function internalSetVisible(visible)
	itrace("internalSetVisible", visible);
	guiSetVisible(browser, visible);
	--setBrowserRenderingPaused (webBrowser, not visible);
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
	isRemote = false;
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

function handleSetRemotePath(path)
	currentPath = path;
	isRemote = true;
	itrace("SetRemotePath", path)
	loadBrowserURL(webBrowser, path)
end

local function handleLoad(mode, x, y, remoteUrl, requestWhitelistUrl)
	selectedMode = mode;
	itrace("handleLoad mode, x, y, remoteUrl", mode, x, y, remoteUrl)
	if(mode == "remote")then
		if(type(remoteUrl) ~= "string")then
			error("Remote url is invalid, got: "..tostring(remoteUrl))
		end
		browser = guiCreateBrowser( sx / 2 - x / 2, sy / 2 - y / 2, x, y, false, true, false)
		webBrowser = guiGetBrowser( browser )
		handleSetVisible(false)

		addEventHandler( "onClientBrowserCreated", webBrowser, 
			function( )
				itrace("onClientBrowserCreated")
				local sourceBrowser = source;
				addEventHandler ( "onClientBrowserNavigate", sourceBrowser, function(...)
					--if(url == remoteUrl)then
						itrace("onClientBrowserNavigate", ...);
						triggerServerEvent("internalBrowserDocumentReady", resourceRoot)
					--end
				end)
				local function handleClientBrowserDocumentReady(url)
					if(string.find(url, remoteUrl))then
						itrace("handleClientBrowserDocumentReady", url);
						triggerServerEvent("internalBrowserDocumentReady", resourceRoot)
						removeEventHandler ( "onClientBrowserDocumentReady", sourceBrowser, handleClientBrowserDocumentReady)
					end
				end

				addEventHandler ( "onClientBrowserDocumentReady", sourceBrowser, handleClientBrowserDocumentReady)
				itrace("Request: ",requestWhitelistUrl)
				if(isBrowserDomainBlocked ( requestWhitelistUrl ))then
					requestBrowserDomains({ requestWhitelistUrl })
					local function handleClientBrowserWhitelistChange(newDomains)
						if newDomains[1] == requestWhitelistUrl then
							triggerServerEvent("internalBrowserCreated", resourceRoot)
							itrace("loadurl", requestWhitelistUrl)
							loadBrowserURL( sourceBrowser, remoteUrl )
							removeEventHandler("onClientBrowserWhitelistChange", root, handleClientBrowserWhitelistChange);
						end
					end
					addEventHandler("onClientBrowserWhitelistChange", root, handleClientBrowserWhitelistChange);
				else
					triggerServerEvent("internalBrowserCreated", resourceRoot)
					itrace("loadurl", remoteUrl)
					loadBrowserURL( sourceBrowser, remoteUrl )
				end
			end
		)
	elseif(mode == "local")then
		browser = guiCreateBrowser( sx / 2 - x / 2, sy / 2 - y / 2, x, y, true, true, false)
		webBrowser = guiGetBrowser( browser )
		handleSetVisible(false)
		setAjaxHandlers(webBrowser)
	
		addEventHandler( "onClientBrowserCreated", webBrowser, 
			function()	
				if(fileExists("index.html"))then
					triggerServerEvent("internalBrowserCreated", resourceRoot)
					loadBrowserURL(source, "http://mta/local/index.html" )
				else
					loadBrowserURL(source, "http://mta/local/error.html" )
				end
			end
		)
	end
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("Load", handleLoad)
	hubBind("ToggleDevTools", handleToggleDevTools)
	hubBind("SetVisible", handleSetVisible)
	hubBind("SetPath", handleSetPath)
	hubBind("SetRemotePath", handleSetRemotePath)
	
	addEvent("rememberFrom")
	addEventHandler("rememberFrom", resourceRoot, handleRememberFrom);
	addEvent("getRememberFrom")
	addEventHandler("getRememberFrom", resourceRoot, handleGetRememberFrom);
	addEvent("forgetFrom")
	addEventHandler("forgetFrom", resourceRoot, handleForgetFrom);
end)
