local ceguiUIProvider = {
	window = function(title, px, py, sx, sy)
		return guiCreateWindow(px, py, sx, sy, title, false)
	end,
	input = function(px, py, sx, sy, parent)
		return guiCreateEdit(px, py + 20, sx, sy, "", false, parent)
	end,
	button = function(text, px, py, sx, sy, parent)
		return guiCreateButton(px, py, sx, sy, text, false, parent)
	end,
	getValue = function(elementHandle)
		return guiGetText(elementHandle)
	end,
	setValue = function(elementHandle, value)
		return guiSetText(elementHandle, value)
	end,

	-- Events:
	onClick = function(elementHandle, callback)
	    addEventHandler ( "onClientGUIClick", elementHandle, function(button, state, absoluteX, absoluteY)
			if button == "left" and state == "up" then
				async(callback)
			end
		end, false)
	end,
	
	-- Miscellaneous
	open = function(windowHandle)
		guiSetVisible(windowHandle, true)
		return true;
	end,
	close = function(windowHandle)
		guiSetVisible(windowHandle, false)
		return true;
	end,
	enable = function(elementHandle)
		guiSetEnabled(elementHandle, true)
	end,
	disable = function(elementHandle)
		guiSetEnabled(elementHandle, false)
	end,
	destroy = function(elementHandle)
		destroyElement(elementHandle)
		return true;
	end,
}

function getCeguiUIProvider()
	return ceguiUIProvider;
end
