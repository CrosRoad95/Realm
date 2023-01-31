local focusedElement = nil
local focusedChildElement = nil

local function getPointFromDistanceRotation(x, y, dist, angle)
    local a = math.rad(90 - angle);
    local dx = math.cos(a) * dist;
    local dy = math.sin(a) * dist;
    return x+dx, y+dy;
end

local function setFocusedElement(newElement, newChildElement)
	if(focusedElement == newElement and focusedChildElement == newChildElement)then
		return;
	end
	focusedElement = newElement;
	focusedChildElement = newChildElement;

	triggerServerEventWithId("internalChangeFocusedElement", focusedElement, newChildElement)
end

local function findNearestElementToPoint(elements, x,y,z)
	local nearestElement = nil;
	local nearestComponent = nil;
	local nearest = 9999999
	local dis = 0;
	for i,v in ipairs(elements)do
		if(getElementData(v, "_focusable"))then
			if(v ~= localPlayer)then
				dis = getDistanceBetweenPoints3D(x,y,z, getElementPosition(v))
				if(nearest > dis)then
					nearestElement = v;
					dis = nearest;
				end
			end
		end
	end
	if(nearestElement ~= nil and getElementType(nearestElement) == "vehicle")then
		local vehicle = nearestElement;
		nearest = 9999999
		for component in pairs(getVehicleComponents(vehicle))do
			dis = getDistanceBetweenPoints3D(x,y,z, getVehicleComponentPosition(vehicle, component, "world"))
			if(nearest > dis)then
				nearestComponent = component;
				nearest = dis;
			end
		end
	end
	return nearestElement, nearestComponent;
end

local function findNearestElementOfIntrestAt(x,y,z, i, d)
	local vehicles = getElementsWithinRange(x,y,z, 2.25, "vehicle", i, d )
	if(#vehicles > 0)then
		return findNearestElementToPoint(vehicles, x,y,z)
	end
	local objects = getElementsWithinRange(x,y,z, 1.5, "object", i, d )
	if(#objects > 0)then
		return findNearestElementToPoint(objects, x,y,z)
	end
	local peds = getElementsWithinRange(x,y,z, 1, "ped", i, d )
	if(#peds > 0)then
		return findNearestElementToPoint(peds, x,y,z)
	end
	return nil;
end

local function updateFocusedElement()
	if(getPedOccupiedVehicle(localPlayer) or getCameraTarget(localPlayer) ~= localPlayer or getElementHealth(localPlayer) <= 0)then
		setFocusedElement(nil)
		return;
	end

	local x,y,z = getElementPosition(localPlayer)
	if(isPedDucked(localPlayer))then
		z = z - 0.5;
	end
	local _,_,r = getElementRotation(localPlayer)
	local px,py,pz = getPointFromDistanceRotation(x, y, 1.25, -r)
	local nearestElement,nearestChildElement = findNearestElementOfIntrestAt(px, py, z, getElementInterior(localPlayer), getElementDimension(localPlayer))
	
	-- DEBUG
	dxDrawLine3D(x,y,z, px, py,z, tocolor(255,0,0), 2)
	if(nearestElement)then
		px,py,pz = getElementPosition(nearestElement);
		dxDrawLine3D(x,y,z, px,py,pz, tocolor(0,255,0), 3)
		if(getElementType(nearestElement) == "vehicle" and nearestChildElement)then
			px,py,pz = getVehicleComponentPosition(nearestElement, nearestChildElement, "world")
			dxDrawLine3D(x,y,z, px,py,pz, tocolor(0,0,255), 3)
		end
	end
	-- END DEBUG

	setFocusedElement(nearestElement,nearestChildElement);
end

addEventHandler("onClientPreRender", root, updateFocusedElement)
