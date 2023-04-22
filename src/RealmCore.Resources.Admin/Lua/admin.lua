local tools = {}
local adminModeEnabled = false;
local enabledTools = {}

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
    if(not tools[name])then
        return false
    end
    return setToolEnabled(name, not tools[name][3])
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
    tools[name] = {enable, disable, false, id} -- enable callback, disable callback, state, id
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("SetAdminEnabled", handleSetAdminEnabled)
	hubBind("AddOrUpdateDebugElements", handleAddOrUpdateDebugElements)
	hubBind("SetTools", handleSetTools)
end)
