local sx, sy = guiGetScreenSize( )
webBrowser = nil
if(dev)then
	local window = guiCreateWindow( sx / 2 - 400, sy / 2 - 300, 800, 600, "Web Browser", false )
	local browser = guiCreateBrowser( 0, 28, 800, 600, false, false, false, window )
	webBrowser = guiGetBrowser( browser )

	addEventHandler( "onClientBrowserCreated", webBrowser, 
		function( )
			loadBrowserURL( source, "http://localhost:5220/" )
		end
	)
	requestBrowserDomains({ "localhost" })
else
	--[[setBrowserAjaxHandler(guiGetBrowser(webBrowser), "index.html", function (get, post)
		return "<h1>asd</h1>";
	end)]]

	setDevelopmentMode(true, true)
	local window = guiCreateWindow( sx / 2 - 400, sy / 2 - 300, 800, 600, "Web Browser", false )
	local browser = guiCreateBrowser( 0, 28, 800, 600, true, true, false, window )
	webBrowser = guiGetBrowser( browser )
	addCommandHandler("devtools", function()
		setDevelopmentMode(true, true)
		toggleBrowserDevTools(webBrowser, true)
	end)
	addEventHandler( "onClientBrowserCreated", webBrowser, 
		function( )
		iprint("created",getTickCount())
			loadBrowserURL( source, "http://mta/local/index.html" )
		end
	)
end
