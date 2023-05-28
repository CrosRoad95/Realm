local sx, sy = guiGetScreenSize();
local webBrowser = nil;
local browser = nil;
local selectedMode = "";

local function handleInvokeVoidAsync(identifier, args)
	triggerServerEvent("cefInvokeVoidAsync", resourceRoot, identifier, args);
end

function handleSetDevelopmentMode(enabled)
	setDevelopmentMode(true, enabled);
end

function handleToggleDevTools(enabled)
	toggleBrowserDevTools(webBrowser, enabled);
end

function handleSetVisible(visible)
	guiSetVisible(browser, visible);
	setBrowserRenderingPaused (webBrowser, not visible);
	showCursor(visible, false);
end

function handleSetPath(path)
	if(selectedMode == "dev")then
		loadBrowserURL(webBrowser, "http://localhost:5220/"..path)
	elseif(selectedMode == "prod")then
		outputChatBox("navigate "..tostring(path))
	end
end

local function handleLoad(mode, x, y)
	selectedMode = mode;
	if(mode == "dev")then
		browser = guiCreateBrowser( sx / 2 - x / 2, sy / 2 - y / 2, x, y, false, false, false)
		webBrowser = guiGetBrowser( browser )
		handleSetVisible(false)

		addEventHandler( "onClientBrowserCreated", webBrowser, 
			function( )
				local sourceBrowser = source;
				if(isBrowserDomainBlocked ( "localhost" ))then
					requestBrowserDomains({ "localhost" })
					local function handleClientBrowserWhitelistChange(newDomains)
						if newDomains[1] == "localhost" then
							loadBrowserURL( sourceBrowser, "http://localhost:5220/" )
							removeEventHandler("onClientBrowserWhitelistChange", root, handleClientBrowserWhitelistChange);
						end
					end
					addEventHandler("onClientBrowserWhitelistChange", root, handleClientBrowserWhitelistChange);
				else
					loadBrowserURL( sourceBrowser, "http://localhost:5220/" )
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
	hubBind("SetDevelopmentMode", handleSetDevelopmentMode)
	hubBind("ToggleDevTools", handleToggleDevTools)
	hubBind("SetVisible", handleSetVisible)
	hubBind("SetPath", handleSetPath)
	
	addEvent("invokeVoidAsync")
	addEventHandler("invokeVoidAsync", resourceRoot, handleInvokeVoidAsync);
end)
