local notifications = {}

addEventHandler("onClientRender", root, function()
    local x, width, height = sx * 0.7777, sx * 0.1719, sy * 0.027;
	local offset = 0.0417;
	for i,v in ipairs(notifications)do
		local y = sy * (0.3222 + (offset * i));
		dxDrawRectangle(x, y, width, height, tocolor(0, 0, 0, 155))
		dxDrawText(v.message, x, y, x + width, y + height, tocolor(255, 255, 255, 255), 1, "sans", "center", "center", false, true)
	end
	if(#notifications > 0)then
		if(notifications[1].visibleUntil < getTickCount())then
			table.remove(notifications, 1)
		end
	end
end)

local function addNotification(message)
	notifications[#notifications + 1] = {
		visibleUntil = getTickCount() + 5000,
		message = message,
	}
end

addEvent("addNotification", true)
addEventHandler("addNotification", localPlayer, function(message)
	addNotification(message);
end)
