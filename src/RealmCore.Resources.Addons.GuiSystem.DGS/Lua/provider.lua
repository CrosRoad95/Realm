﻿local offset = -20;
local dgsUIProvider = {
	-- Elements
	window = function(title, px, py, sx, sy)
		return dgsCreateWindow(px, py, sx, sy, title, false, 0xFFFFFFFF, 25, nil, 0xC8141414, nil, 0x96141414, 5, true)
	end,
	input = function(px, py, sx, sy, parent)
		return dgsCreateEdit(px, py + offset, sx, sy, "", false, parent)
	end,
	button = function(text, px, py, sx, sy, parent)
		return dgsCreateButton(px, py + offset, sx, sy, text, false, parent)
	end,
	label = function(text, px, py, sx, sy, parent)
		return dgsCreateLabel(px, py + offset, sx, sy, text, false, parent)
	end,
	checkbox = function(text, px, py, sx, sy, selected, parent)
		return dgsCreateCheckBox(px, py + offset, sx, sy, text, selected, false, parent)
	end,
	scrollPane = function(x, y, width, height, parent)
		return dgsCreateScrollPane(x, y, width, height, false, parent)
	end,
	tabPanel = function(x, y, width, height, parent)
		return dgsCreateTabPanel(x, y, width, height, false, parent)
	end,
	tab = function(text, parent)
		return dgsCreateTab(text, parent)
	end,

	-- Getters, setters
	getValue = function(elementHandle)
		return dgsGetText(elementHandle)
	end,
	setValue = function(elementHandle, value)
		return dgsSetText(elementHandle, value or "")
	end,
	setMasked = function(elementHandle, enabled)
		return dgsEditSetMasked(elementHandle, true)
	end,
	getSelected = function(elementHandle)
		if(isElement(elementHandle) and getElementType(elementHandle) == "dgs-dxcheckbox")then
			return dgsCheckBoxGetSelected(elementHandle)
		end
		return false;
	end,
	setSelected = function(elementHandle, selected)
		if(isElement(elementHandle) and getElementType(elementHandle) == "dgs-dxcheckbox")then
			dgsCheckBoxSetSelected(elementHandle, selected)
			return true;
		end
		return false;
	end,

	-- Events:
	onClick = function(elementHandle, callback)
	    addEventHandler ( "onDgsMouseClick", elementHandle, function(button, state, absoluteX, absoluteY)
			if button == "left" and state == "up" then
				async(callback)
			end
		end, false)
	end,
	
	-- Miscellaneous
	open = function(windowHandle)
		dgsSetVisible(windowHandle, true)
		return true;
	end,
	close = function(windowHandle)
		dgsSetVisible(windowHandle, false)
		return true;
	end,
	enable = function(elementHandle)
		dgsSetEnabled(elementHandle, true)
	end,
	disable = function(elementHandle)
		dgsSetEnabled(elementHandle, false)
	end,
	destroy = function(elementHandle)
		if(isElement(elementHandle))then
			destroyElement(elementHandle)
			return true;
		end
		return false;
	end,
	getSize = function(elementHandle)
		return dgsGetSize(elementHandle)
	end,
	setWindowPosition = function(windowHandle, x, y)
		dgsSetPosition(windowHandle, x, y, false)
	end,
	centerWindow = function(windowHandle)
	    local screenW, screenH = guiGetScreenSize()
		local windowW, windowH = dgsGetSize(windowHandle, false)
		local x, y = (screenW - windowW) / 2, (screenH - windowH) / 2
		dgsSetPosition(windowHandle, x, y, false)
		return true;
	end,
	setHorizontalAlign = function(labelHandle, align, wordWrap)
		dgsLabelSetHorizontalAlign(labelHandle, align, wordWrap)
	end,
	setVerticalAlign = function(labelHandle, align)
		dgsLabelSetVerticalAlign(labelHandle, align)
	end,
}

addEventHandler("onClientResourceStart", resourceRoot, function()
	registerGuiProvider("dgs", dgsUIProvider)
end)
