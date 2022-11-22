local adminToolsEnabled = false;

addEvent("internalSetAdminToolsEnabled", true)
addEvent("internalOnAdminToolsEnabled", false)
addEvent("internalOnAdminToolsDisabled", false)

addEventHandler("internalSetAdminToolsEnabled", localPlayer, function(enabled)
    outputChatBox("internalSetAdminToolsEnabled"..tostring(enabled))
    if(adminToolsEnabled == enabled)then
        return
    end
    if(enabled)then
        triggerEvent("internalOnAdminToolsEnabled", localPlayer)
    else
        triggerEvent("internalOnAdminToolsDisabled", localPlayer)
    end
end)
