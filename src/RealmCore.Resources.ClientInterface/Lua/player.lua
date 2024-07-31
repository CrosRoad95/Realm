local sx,sy = guiGetScreenSize()
--local hoveredElement = nil;

triggerServerEventWithId("sendLocalizationCode", getLocalization().code)
triggerServerEventWithId("sendScreenSize", guiGetScreenSize())

function handleSetClipboard(content)
	setClipboard(content)
end

function handleSetDevelopmentModeEnabled(enabled)
	setDevelopmentMode(enabled, enabled)
end

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

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("SetClipboard", handleSetClipboard);
	hubBind("SetDevelopmentModeEnabled", handleSetDevelopmentModeEnabled);
end)
