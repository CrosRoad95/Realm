local sx,sy = guiGetScreenSize()
--local hoveredElement = nil;
setElementData(localPlayer, "translations", {}, false)
triggerServerEventWithId("sendLocalizationCode", getLocalization().code)
triggerServerEventWithId("sendScreenSize", guiGetScreenSize())

addEvent("updateTranslation", true)
addEventHandler("updateTranslation", localPlayer, function(translations)
	setElementData(localPlayer, "translations", translations[1], false)
end)

addEvent("internalSetClipboard", true)
addEventHandler("internalSetClipboard", localPlayer, function(content)
	setClipboard(content)
end)

addEvent("internalSetDevelopmentModeEnabled", true)
addEventHandler("internalSetDevelopmentModeEnabled", localPlayer, function(enabled)
	setDevelopmentMode(enabled, enabled)
end)

--[[
addEventHandler( "onClientCursorMove", root,
    function ( _, _, x, y)
        if(isCursorShowing())then
            local startX,startY,startZ = getWorldFromScreenPosition(sx/2, sy/2, 0);
            local endX,endY,endZ = getWorldFromScreenPosition(x, y, 250)
            local hit,hitX,hitY,hitZ,hitElement = processLineOfSight(startX,startY,startZ,endX,endY,endZ, true, true, true, true, true, false, false, false, localPlayer, false, false)
            hoveredElement = hitElement;
        end
    end
);]]

addEventHandler ( "onClientClick", root, function(button, state, absoluteX, absoluteY, worldX, worldY, worldZ, clickedElement)
    triggerServerEventWithId("clickedElementChanged", clickedElement)
end)

addCommandHandler("boom", function()
    outputChatBox("boom")
    local x,y,z = getElementPosition(localPlayer)
    createExplosion (x,y + 20,z, 1)
end)
addCommandHandler("boomfar", function()
    outputChatBox("boom")
    createExplosion (1000, 1000, 1000, 1)
end)