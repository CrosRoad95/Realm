local enabledHuds = {}
local huds = {}
local assets = {}

local function renderHud(hudData)
	local x,y = unpack(hudData.position)
	for i,v in ipairs(hudData.elements)do
		if(v[1] == "text")then
			dxDrawText(v[3], v[4] + x, v[5] + y, v[4] + v[6] + x, v[5] + v[7] + y, v[8], v[9], v[10], v[11], v[12], v[13])
		elseif(v[1] == "rectangle")then
			dxDrawRectangle(v[3] + x, v[4] + y, v[5], v[6], v[7])
		end
	end
end

local function prepareAsset(asset)
	local assetType = asset[1];
	if(assetType == "Font")then
		if(not assets[asset[2]])then
			return requestAsset(asset[2])
		end
	end
	return asset;
end

local function prepareElements(hudData)
	for i,v in ipairs(hudData.elements)do
		if(v[1] == "text")then
			local asset = v[11];
			if(type(asset) == "table")then
				v[11] = prepareAsset(asset);
			end
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

local function setHudState(hudData, newState)
	for i,v in ipairs(hudData.elements)do
		if(newState[v[2]])then
			if(v[1] == "text")then
				v[3] = newState[v[2]]
			end
		end
	end
end

addEvent("setHudVisible", true)
addEventHandler("setHudVisible", localPlayer, function(hudId, enabled)
	enabledHuds[hudId] = enabled;
end)

addEvent("setHudPosition", true)
addEventHandler("setHudPosition", localPlayer, function(hudId, x, y)
	huds[hudId].position = {x, y};
end)

addEvent("setHudState", true)
addEventHandler("setHudState", localPlayer, function(hudId, newHudState)
	setHudState(huds[hudId], newHudState)
end)

addEvent("createHud", true)
addEventHandler("createHud", localPlayer, function(hudId, x, y, elements)
	huds[hudId] = {
		position = {x,y},
		elements = elements,
	}
	prepareElements(huds[hudId]);
end)

addEvent("removeHud", true)
addEventHandler("removeHud", localPlayer, function(hudId)
	huds[hudId] = nil;
	enabledHuds[hudId] = nil;
end)

addEventHandler("onClientRender", root, renderHuds);