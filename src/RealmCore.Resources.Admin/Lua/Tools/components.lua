local function enable()

end

local function disable()
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addTool("Debug component", enable, disable, 1)
end)