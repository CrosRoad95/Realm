local tools = {}
local adminToolsEnabled = false;

addEvent("internalSetAdminToolsEnabled", true)
addEvent("internalOnAdminToolsEnabled", false)
addEvent("internalOnAdminToolsDisabled", false)

addEventHandler("internalSetAdminToolsEnabled", localPlayer, function(enabled)
    if(adminToolsEnabled == enabled)then
        return
    end
    if(enabled)then
        triggerEvent("internalOnAdminToolsEnabled", localPlayer)
    else
        triggerEvent("internalOnAdminToolsDisabled", localPlayer)
    end
end)
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