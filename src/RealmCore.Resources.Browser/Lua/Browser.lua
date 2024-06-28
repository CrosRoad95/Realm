local sx, sy = guiGetScreenSize();
local webBrowser = nil;
local browser = nil;
local isRemote = false;
local trace = false;
local baseRemoteUrl = "";

local function itrace(...)
	if(trace)then
		iprint(getTickCount(), ...)
	end
end

function handleToggleDevTools(enabled)
	setDevelopmentMode(getDevelopmentMode(), enabled)
	toggleBrowserDevTools(webBrowser, enabled);
end

function handleSetVisible(visible)
	itrace("handleSetVisible", visible)
	guiSetVisible(browser, visible);
	--setBrowserRenderingPaused(webBrowser, not visible);
	showCursor(visible, false);
end

function handleSetPath(path)
	itrace("handleSetPath", path)
	loadBrowserURL(webBrowser, path)
end

local function createLocalBrowser(x, y)
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

local function createRemoteBrowser(x, y, remoteUrl, requestWhitelistUrl)
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
end

local function handleLoad(x, y, remoteUrl, requestWhitelistUrl)
	itrace("handleLoad, x, y, remoteUrl", x, y, remoteUrl)
	baseRemoteUrl = remoteUrl;
	createRemoteBrowser(x, y, remoteUrl, requestWhitelistUrl)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("Load", handleLoad)
	hubBind("ToggleDevTools", handleToggleDevTools)
	hubBind("SetVisible", handleSetVisible)
	hubBind("SetPath", handleSetPath)
end)
