local animationTime = 300; -- in miliseconds
local selectedTool = 1;
local screenX,screenY = guiGetScreenSize()
local opened = false
local itemHeight = 25;
local rightOffset = 300;
local animationStart = getTickCount() - animationTime
local currentlySelectedTool = nil
local loggedIn = false;

local colors = {
	disabled = tocolor(255, 255, 255, 255),
	enabled = tocolor(0, 255, 0, 255),
	selected = tocolor(0, 255, 255, 255),
}

function math.clamp(number, min, max)
	if number < min then
		return min
	elseif number > max then
		return max    
	end
	return number
end

local function render()
	local tools = getTools()
	local toolsCount = #tools
	local px,py = screenX - rightOffset, screenY - (toolsCount + 2) * itemHeight;
	local t = math.min(getTickCount() - animationStart, animationTime) / animationTime
	local height = itemHeight * toolsCount + itemHeight
	local offset = getEasingValue(t, "InOutQuad") * height
	if(opened)then
		offset = height + -offset
	end
	local isEnabled = false;
	for i,name in ipairs(tools)do
		isEnabled = isToolEnabled(name)
		local color = colors.disabled
		local itemX, itemY = px, py + offset + i * itemHeight
		if(i == selectedTool)then
			color = colors.selected
			currentlySelectedTool = name
		else
			color = isEnabled and colors.enabled or colors.disabled
		end
		dxDrawText("["..(isEnabled and "X" or "  ").."] ".. name, itemX, itemY, itemX + rightOffset, itemY + 25, color, 1.4, "sans", "left", "center")
	end
end

local function toggling()
	if(currentlySelectedTool ~= nil)then
		toggleTool(currentlySelectedTool)
	end
end

local function navigation(key, state)
	local toolsCount = #getTools()
	if(key == "arrow_d")then
		selectedTool = selectedTool + 1
		if(selectedTool > toolsCount)then
			selectedTool = 1
		end
	else
		selectedTool = selectedTool - 1
		if(selectedTool < 1)then
			selectedTool = toolsCount
		end
	end
end

local function openCloseDialog(key, state)
	if(not loggedIn)then
		return;
	end
	if(state == "down")then
		opened = true
		bindKey("space", "down", toggling)
		bindKey("arrow_u", "down", navigation)
		bindKey("arrow_d", "down", navigation)
	else
		opened = false
		unbindKey("space", "down", toggling)
		unbindKey("arrow_u", "down", navigation)
		unbindKey("arrow_d", "down", navigation)
	end

	animationStart = getTickCount()
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addEventHandler("onLoggedIn", localPlayer, function()
		loggedIn = true;
	end)
	addEventHandler("internalOnAdminToolsEnabled", localPlayer, function()
		addEventHandler("onClientRender", root, render)
		bindKey("z", "both", openCloseDialog)
	end)

	addEventHandler("internalOnAdminToolsDisabled", localPlayer, function()
		removeEventHandler("onClientRender", root, render)
		unbindKey("z", "both", openCloseDialog)
		if(opened)then
			unbindKey("space", "both", onToggleTool)
			unbindKey("arrow_u", "down", navigation)
			unbindKey("arrow_d", "down", navigation)
			opened = false
		end
	end)
end)