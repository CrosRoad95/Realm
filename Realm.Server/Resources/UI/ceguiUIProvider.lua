local ceguiUIProvider = {
	window = function(title, px, py, sx, sy)
		return guiCreateWindow(px, py, sx, sy, title, false)
	end,
	open = function(windowHandle)
		guiSetVisible(windowHandle, true)
		return true;
	end,
	close = function(windowHandle)
		guiSetVisible(windowHandle, false)
		return true;
	end,
}

function getCeguiUIProvider()
	return ceguiUIProvider;
end
