local function drawWorld()
	dxDrawText("DRAW WORLD", 0,30,100, 100)
end

local function enableDrawWorld()
	addEventHandler("onClientRender", root, drawWorld)
end

local function disableDrawWorld()
	removeEventHandler("onClientRender", root, drawWorld)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addTool("Debug world", enableDrawWorld, disableDrawWorld)
end)
