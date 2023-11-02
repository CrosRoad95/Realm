local tools = {}
local adminModeEnabled = false;
local enabledTools = {}
local elements = {}
local spawnMarkers = {}
local toolIdsInUse = {}
local elementsComponents = {}

function getElements()
    return elements;
end

function getSpawnMarkers()
    return spawnMarkers;
end

function getElementsComponents()
    return elementsComponents;
end

addEvent("internalOnAdminModeEnabled", false)
addEvent("internalOnAdminModeDisabled", false)

function handleSetAdminEnabled(enabled)
    if(adminModeEnabled == enabled)then
        return
    end
    if(enabled)then
        triggerEvent("internalOnAdminModeEnabled", localPlayer)
    else
        triggerEvent("internalOnAdminModeDisabled", localPlayer)
    end
end

function handleSetTools(newEnabledTools)
    enabledTools = {}
    for i,v in ipairs(newEnabledTools)do
        enabledTools[v] = v
    end
end

function handleAddElement(element)
    iprint("element", element)
end

function getTools()
    local toolsNames = {}
    for name,v in pairs(tools)do
        if(enabledTools[v[4]])then
            toolsNames[#toolsNames + 1] = name
        end
    end
    table.sort(toolsNames)
    return toolsNames
end

function toggleTool(name)
    local tool = tools[name];
    if(not tool)then
        return false
    end
    local newState = not tool[3];
    triggerServerEvent("internalSetToolState", resourceRoot, tool[4], newState)
    return setToolEnabled(name, newState);
end

function isToolEnabled(name)
    if(not tools[name])then
        return false
    end
    return tools[name][3]
end

function setToolEnabled(name, enabled)
    local tool = tools[name];
    if(not tool)then
        return false
    end
    if(not enabledTools[tool[4]])then
        return
    end
    if(enabled ~= tool[3])then
        if(enabled)then
            tool[1]()
        else
            tool[2]()
        end
        tool[3] = enabled
        return true
    end
    return false
end

function addTool(name, enable, disable, id)
    if(toolIdsInUse[id])then
        error("Admin tool id "..id.." is already in use");
    end
    toolIdsInUse[id] = true
    tools[name] = {enable, disable, false, id} -- enable callback, disable callback, state, id
end

local function handleAddOrUpdateElement(element)
    elements[element.debugId] = element
end

function handleAddOrUpdateElements(elementOrElements)
	if(#elementOrElements > 0)then
		for i,v in ipairs(elementOrElements)do
			handleAddOrUpdateElement(v)
		end
	else
		handleAddOrUpdateElement(elementOrElements)
	end
end

function handleClearElements()
    elements = {};
end

function handleSetSpawnMarkers(data)
    spawnMarkers = data;
end

function handleClearSpawnMarkers()
    spawnMarkers = {};
end

function handleUpdateElementsComponents(data)
    elementsComponents = data;
end

function handleClearElementsComponents()
    elementsComponents = {}
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("SetAdminEnabled", handleSetAdminEnabled)
	hubBind("SetTools", handleSetTools)
	hubBind("AddOrUpdateElement", handleAddOrUpdateElements)
	hubBind("ClearElements", handleClearElements)
	hubBind("SetSpawnMarkers", handleSetSpawnMarkers)
	hubBind("ClearSpawnMarkers", handleClearSpawnMarkers)
	hubBind("UpdateElementsComponents", handleUpdateElementsComponents)
	hubBind("ClearElementsComponents", handleClearElementsComponents)
end)
