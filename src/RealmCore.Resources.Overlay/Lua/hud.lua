local visibleHuds = {}
local huds = {}
local huds3d = {}
local assets = {}
local hud3dResolution = 128; -- 128 pixels per 1m
local visibleCounter = 0;
local renderHuds;

local function isHudVisible(hudId)
	return visibleHuds[hudId] and true or false
end

local function setHudVisibleCore(hudId, visible)
	if(isHudVisible(hudId) ~= visible)then
		if(visible)then
			visibleCounter = visibleCounter + 1;
			visibleHuds[hudId] = true;
		else
			visibleCounter = visibleCounter - 1;
			visibleHuds[hudId] = nil;
		end
	else
		return false;
	end

	if(visible)then
		if(visibleCounter == 1)then
			addEventHandler("onClientRender", root, renderHuds);
		end
	else
		if(visibleCounter == 0)then
			removeEventHandler("onClientRender", root, renderHuds);
		end
	end
end

function getElementSpeed(theElement, unit)
    local elementType = getElementType(theElement)
    unit = unit == nil and 0 or ((not tonumber(unit)) and unit or tonumber(unit))
    local mult = (unit == 0 or unit == "m/s") and 50 or ((unit == 1 or unit == "km/h") and 180 or 111.84681456)
    return (Vector3(getElementVelocity(theElement)) * mult).length
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

local function renderHud(position, elements)
	local x,y = unpack(position)
	for i,v in ipairs(elements)do
		if(v[1] == "text")then
			local content = v[3];
			if(content[1] == "constant")then
				dxDrawText(content[2], v[4] + x, v[5] + y, v[4] + v[6] + x, v[5] + v[7] + y, v[8], v[9], v[10], v[11] or "sans", v[12], v[13])
			elseif(content[1] == "computed")then
				if(content[2] == "vehicleSpeed")then
					local vehicle = getPedOccupiedVehicle(localPlayer)
					if(vehicle and getVehicleController(vehicle) == localPlayer)then
						local speed = getElementSpeed(vehicle, "km/s")
						dxDrawText(string.format("%ikm/h", speed), v[4] + x, v[5] + y, v[4] + v[6] + x, v[5] + v[7] + y, v[8], v[9], v[10], v[11] or "sans", v[12], v[13])
					end
				end
			end
		elseif(v[1] == "rectangle")then
			dxDrawRectangle(v[3] + x, v[4] + y, v[5], v[6], v[7])
		end
	end
end

local function prepareAsset(assetInfo)
	local assetType = assetInfo[1];
	if(assetType == "FileSystemFont")then
		if(not assets[assetInfo[2]])then
			return requestAsset(assetInfo[2])
		end
	elseif(assetType == "MtaFont")then
		return assetInfo[2]
	end
	return assetInfo;
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

local function renderHud3d(elements, oldrt)
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

function renderHuds()
	if(isPlayerMapVisible())then
		return;
	end

	for hudId,hud in pairs(huds)do
		if(isHudVisible(hudId))then
			renderHud(hud.position, hud.elements)
		end
	end
end

local function renderHuds3d()
	local h;
	for i,v in pairs(huds3d)do
		h = v.size[2] / hud3dResolution;
		if(v.dirtyState or not isElement(v.element))then
			local newrt, sx, sy = renderHud3d(v.elements, v.element)
			v.element = newrt;
			v.size = {sx, sy}
			v.dirtyState = false;
		end
		dxDrawMaterialLine3D(v.position[1], v.position[2], v.position[3] + h/2, v.position[1], v.position[2], v.position[3] - h/2, false, v.element, v.size[1] / hud3dResolution)
	end
end

local function handleAddNotification(message)
	notifications[#notifications + 1] = {
		visibleUntil = getTickCount() + 5000,
		message = message,
	}
end

local function setHudStateCore(elements, newState)
	for i,v in ipairs(elements)do
		if(newState[v[2]])then
			if(v[1] == "text")then
				v[3][2] = newState[v[2]]
			end
		end
	end
end

local function handleSetHudVisible(hudId, enabled)
	if(huds[hudId])then
		setHudVisibleCore(hudId, enabled);
	else
		outputDebugString("Failed to setHudVisible, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleSetHudPosition(hudId, x, y)
	if(huds[hudId])then
		huds[hudId].position = {x, y};
	else
		outputDebugString("Failed to setHudPosition, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleSetHudState(hudId, newHudState)
	if(huds[hudId])then
		setHudStateCore(huds[hudId].elements, newHudState)
	else
		outputDebugString("Failed to setHudState, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleSetHud3dState(hudId, newHudState)
	if(huds3d[hudId])then
		setHudStateCore(huds3d[hudId].elements, newHudState)
		huds3d[hudId].dirtyState = true;
	else
		outputDebugString("Failed to setHud3dState, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleCreateHud(hudId, x, y, elements)
	if(huds[hudId])then
		outputDebugString("Failed to createHud, hud of id: '"..tostring(hudId).."' already exists.", 1);
		return;
	end

	huds[hudId] = {
		position = {x,y},
		elements = elements,
	}
	prepareElements(elements);
end

local function handleCreateHud3d(hudId, x, y, z, elements)
	if(huds3d[hudId])then
		outputDebugString("Failed to createHud3d, hud of id: '"..tostring(hudId).."' already exists.", 1);
		return;
	end

	huds3d[hudId] = {
		elements = elements,
		position = {x,y,z},
		size = {-1, -1},
		element = nil,
		dirtyState = true,
	}
end

local function handleRemoveHud(hudId)
	if(huds[hudId])then
		huds[hudId] = nil;
		setHudVisibleCore(hudId, false);
	else
		outputDebugString("Failed to removeHud, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleRemoveHud3d(hudId)
	if(huds3d[hudId])then
		destroyElement(huds3d[hudId].element)
		huds3d[hudId] = nil;
		setHudVisibleCore(hudId, false);
	else
		outputDebugString("Failed to removeHud3d, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("AddNotification", handleAddNotification)
	hubBind("SetHudVisible", handleSetHudVisible)
	hubBind("SetHudPosition", handleSetHudPosition)
	hubBind("SetHudState", handleSetHudState)
	hubBind("SetHud3dState", handleSetHud3dState)
	hubBind("CreateHud", handleCreateHud)
	hubBind("CreateHud3d", handleCreateHud3d)
	hubBind("RemoveHud", handleRemoveHud)
	hubBind("RemoveHud3d", handleRemoveHud3d)
	hubBind("AddDisplay3dRing", handleAddDisplay3dRing)
	hubBind("RemoveDisplay3dRing", handleRemoveDisplay3dRing)
	
	addEventHandler("onClientRender", root, renderHuds3d); -- TODO:
end)
