local enabledHuds = {}
local huds = {}

local function renderHud(hudData)
	local x,y = unpack(hudData.position)
	for i,v in ipairs(hudData.elements)do
		if(v[1] == "text")then
			dxDrawText(v[3], v[4] + x, v[5] + y, v[4] + v[6] + x, v[5] + v[7] + y, v[8], v[9], v[10], v[11], v[12], v[13])
		elseif(v[1] == "rectangle")then
			dxDrawRectangle(v[3] + x, v[4] + y, v[5] + x, v[6] + y, v[7])
		end
	end
end

local function renderHuds()
	for i,v in pairs(huds)do
		if(enabledHuds[i])then
			renderHud(v)
		end
	end
end

local function addNotification(message)
	notifications[#notifications + 1] = {
		visibleUntil = getTickCount() + 5000,
		message = message,
	}
end

addEvent("setHudVisible", true)
addEventHandler("setHudVisible", localPlayer, function(hudId, enabled)
	enabledHuds[hudId] = enabled;
end)

addEvent("createHud", true)
addEventHandler("createHud", localPlayer, function(hudId, x, y, elements)
	huds[hudId] = {
		position = {x,y},
		elements = elements,
	}
end)

addEvent("removeHud", true)
addEventHandler("removeHud", localPlayer, function(hudId)
	huds[hudId] = nil;
	enabledHuds[hudId] = nil;
end)

addEventHandler("onClientRender", root, renderHuds);