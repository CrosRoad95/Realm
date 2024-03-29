﻿local ceguiUIProvider = {
	-- Elements
	window = function(title, px, py, sx, sy)
		return guiCreateWindow(px, py, sx, sy, title, false)
	end,
	input = function(px, py, sx, sy, parent)
		return guiCreateEdit(px, py, sx, sy, "", false, parent)
	end,
	button = function(text, px, py, sx, sy, parent)
		return guiCreateButton(px, py, sx, sy, text, false, parent)
	end,
	label = function(text, px, py, sx, sy, parent)
		return guiCreateLabel(px, py, sx, sy, text, false, parent)
	end,
	checkbox = function(text, px, py, sx, sy, selected, parent)
		return guiCreateCheckBox(px, py, sx, sy, text, selected, false, parent)
	end,
	scrollPane = function(x, y, width, height, parent)
		return guiCreateScrollPane(x, y, width, height, false, parent)
	end,
	tabPanel = function(x, y, width, height, parent)
		return guiCreateTabPanel(x, y + 20, width, height - 20, false, parent)
	end,
	tab = function(text, parent)
		return guiCreateTab(text, parent)
	end,

	-- Getters, setters
	getValue = function(elementHandle)
		return guiGetText(elementHandle)
	end,
	setValue = function(elementHandle, value)
		return guiSetText(elementHandle, value or "")
	end,
	setMasked = function(elementHandle, enabled)
		return guiEditSetMasked(elementHandle, true)
	end,
	getSelected = function(elementHandle)
		if(isElement(elementHandle) and getElementType(elementHandle) == "gui-checkbox")then
			return guiCheckBoxGetSelected(elementHandle)
		end
		return false;
	end,
	setSelected = function(elementHandle, selected)
		if(isElement(elementHandle) and getElementType(elementHandle) == "gui-checkbox")then
			guiCheckBoxSetSelected(elementHandle, selected)
			return true;
		end
		return false;
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
		if(isElement(elementHandle))then
			destroyElement(elementHandle)
			return true;
		end
		return false;
	end,
	getSize = function(elementHandle)
		return guiGetSize(elementHandle)
	end,
	setWindowPosition = function(windowHandle, x, y)
		guiSetPosition(windowHandle, x, y, false)
	end,
	centerWindow = function(windowHandle)
	    local screenW, screenH = guiGetScreenSize()
		local windowW, windowH = guiGetSize(windowHandle, false)
		local x, y = (screenW - windowW) / 2, (screenH - windowH) / 2
		guiSetPosition(windowHandle, x, y, false)
		return true;
	end,
	setHorizontalAlign = function(labelHandle, align, wordWrap)
		guiLabelSetHorizontalAlign(labelHandle, align, wordWrap)
	end,
	setVerticalAlign = function(labelHandle, align)
		guiLabelSetHorizontalAlign(labelHandle, align)
	end,
}

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGuiProvider("cegui", ceguiUIProvider)
end)
