local drawComponents = false;

function isComponentDrawingEnabled()
	return drawComponents;
end

local function stringifyComponent(component)
	local result = component.name;
	if(component.data)then
		local t = {}
		for i,v in pairs(component.data)do
			t[#t + 1] = "- "..i..": "..v
		end
		result = result.."\n"..table.concat(t, "\n")
	end
	return result;
end

function getComponents(id)
	local components = getElementsComponents()[id];
	if(not components)then
		return "-";
	end
	
	local t = {}
	for i,v in ipairs(components)do
		t[i] = stringifyComponent(v);
	end
	return table.concat(t, "\n");
end

local function enable()
	drawComponents = true;
end

local function disable()
	drawComponents = false;
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addTool("Debug component", enable, disable, 1)
end)