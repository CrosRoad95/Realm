local enabledHuds = {}
local huds = {}
local huds3d = {}
local assets = {}
local hud3dResolution = 128; -- 128 pixels per 1m

function getElementSpeed(theElement, unit)
    local elementType = getElementType(theElement)
    unit = unit == nil and 0 or ((not tonumber(unit)) and unit or tonumber(unit))
    local mult = (unit == 0 or unit == "m/s") and 50 or ((unit == 1 or unit == "km/h") and 180 or 111.84681456)
    return (Vector3(getElementVelocity(theElement)) * mult).length
end

local function renderHud(position, elements)
	local x,y = unpack(position)
	for i,v in ipairs(elements)do
		if(v[1] == "text")then
			dxDrawText(v[3], v[4] + x, v[5] + y, v[4] + v[6] + x, v[5] + v[7] + y, v[8], v[9], v[10], v[11] or "sans", v[12], v[13])
		elseif(v[1] == "computedValue")then
			if(v[3] == "VehicleSpeed")then
				local vehicle = getPedOccupiedVehicle(localPlayer)
				if(vehicle and getVehicleController(vehicle) == localPlayer)then
					local speed = getElementSpeed(vehicle, "km/s")
					dxDrawText(string.format("%ikm/h", speed), v[4] + x, v[5] + y, v[4] + v[6] + x, v[5] + v[7] + y, v[8], v[9], v[10], v[11] or "sans", v[12], v[13])
				end
			end
		elseif(v[1] == "rectangle")then
			dxDrawRectangle(v[3] + x, v[4] + y, v[5], v[6], v[7])
		end
	end
end

local function calculateBoundingBox(position, elements)
	local x,y = unpack(position)
	local maxX, maxY = 0,0
	for i,v in ipairs(elements)do
		maxX = math.max(maxX, v[4] + x, v[4] + v[6] + x)
		maxY = math.max(maxY, v[5] + y, v[5] + v[7] + y)
	end
	return maxX + x, maxY + y
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

local function prepareElements(elements)
	for i,v in ipairs(elements)do
		if(v[1] == "text" or v[1] == "computedValue")then
			local asset = v[11];
			if(type(asset) == "table")then
				v[11] = prepareAsset(asset);
			end
		end
	end
end

local function rerenderHud3d(elements, oldrt)
	local rt;
	prepareElements(elements);
	local sx,sy = calculateBoundingBox({0, 0}, elements)
	if(oldrt)then
		local osx, osy = dxGetMaterialSize(oldrt);
		if(osx == sx and osy == sy)then
			rt = oldrt
		else
			destroyElement(oldrt)
			rt = dxCreateRenderTarget(sx, sy, false)
		end
	else
		rt = dxCreateRenderTarget(sx, sy, false)
	end
    dxSetRenderTarget(rt)
	renderHud({0,0}, elements)
    dxSetRenderTarget()
	return rt, sx, sy;
end

local function renderHuds()
	for i,v in pairs(huds)do
		if(enabledHuds[i])then
			renderHud(v.position, v.elements)
		end
	end
	local h;
	for i,v in pairs(huds3d)do
		h = v.size[2] / hud3dResolution;
		if(v.rerender or not isElement(v.element))then
			local newrt, sx, sy = rerenderHud3d(v.elements, v.element)
			v.element = newrt;
			v.size = {sx, sy}
			v.rerender = false;
		end
		dxDrawMaterialLine3D(v.position[1], v.position[2], v.position[3] + h/2, v.position[1], v.position[2], v.position[3] - h/2, false, v.element, v.size[1] / hud3dResolution)
	end
end

local function addNotification(message)
	notifications[#notifications + 1] = {
		visibleUntil = getTickCount() + 5000,
		message = message,
	}
end

local function setHudState(elements, newState)
	for i,v in ipairs(elements)do
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
	setHudState(huds[hudId].elements, newHudState)
end)

addEvent("setHud3dState", true)
addEventHandler("setHud3dState", localPlayer, function(hudId, newHudState)
	setHudState(huds3d[hudId].elements, newHudState)
	huds3d[hudId].rerender = true;
end)

addEvent("createHud", true)
addEventHandler("createHud", localPlayer, function(hudId, x, y, elements)
	huds[hudId] = {
		position = {x,y},
		elements = elements,
	}
	prepareElements(elements);
end)

addEvent("createHud3d", true)
addEventHandler("createHud3d", localPlayer, function(hudId, elements, x, y, z)
	huds3d[hudId] = {
		elements = elements,
		position = {x,y,z},
		size = {-1, -1},
		element = nil,
		rerender = true,
	}
end)

addEvent("removeHud", true)
addEventHandler("removeHud", localPlayer, function(hudId)
	huds[hudId] = nil;
	enabledHuds[hudId] = nil;
end)

addEvent("removeHud3d", true)
addEventHandler("removeHud3d", localPlayer, function(hudId)
	destroyElement(huds3d[hudId].element)
	huds3d[hudId] = nil;
	enabledHuds[hudId] = nil;
end)

addEventHandler("onClientRender", root, renderHuds);