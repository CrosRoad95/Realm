local tools = {}
local adminModeEnabled = false;

addEvent("internalSetAdminEnabled", true)
addEvent("internalOnAdminModeEnabled", false)
addEvent("internalOnAdminModeDisabled", false)

function handleInternalSetAdminEnabled(enabled)
    if(adminModeEnabled == enabled)then
        return
    end
    if(enabled)then
        triggerEvent("internalOnAdminModeEnabled", localPlayer)
    else
        triggerEvent("internalOnAdminModeDisabled", localPlayer)
    end
end

function getTools()
    local toolsNames = {}
    for name in pairs(tools)do
        toolsNames[#toolsNames + 1] = name
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
    if(not tools[name])then
        return false
    end
    if(enabled ~= tools[name][3])then
        if(enabled)then
            tools[name][1]()
        else
            tools[name][2]()
        end
        tools[name][3] = enabled
        return true
    end
    return false
end

function addTool(name, enable, disable)
    tools[name] = {enable, disable, false} -- enable callback, disable callback, state
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("InternalSetAdminEnabled", handleInternalSetAdminEnabled)
	hubBind("InternalAddOrUpdateDebugElements", handleInternalAddOrUpdateDebugElements)
end)
