local focusedElement = nil

local function getPointFromDistanceRotation(x, y, dist, angle)
    local a = math.rad(90 - angle);
    local dx = math.cos(a) * dist;
    local dy = math.sin(a) * dist;
    return x+dx, y+dy;
end

local function setFocusedElement(newElement)
	if(focusedElement == newElement)then
		return;
	end
	focusedElement = newElement;

	triggerServerEventWithId("internalChangeFocusedElement", focusedElement)
end

local function findNearestElementToPoint(elements, x,y,z)
	local nearestElement = nil;
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
	return nearestElement;
end

local function findNearestElementOfIntrestAt(x,y,z, i, d)
	local vehicles = getElementsWithinRange(x,y,z, 2.5, "vehicle", i, d )
	if(#vehicles > 0)then
		return findNearestElementToPoint(vehicles, x,y,z)
	end
	local objects = getElementsWithinRange(x,y,z, 2, "object", i, d )
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
	if(getPedOccupiedVehicle(localPlayer) or getCameraTarget(localPlayer) ~= localPlayer)then
		setFocusedElement(nil)
		return;
	end

	local x,y,z = getElementPosition(localPlayer)
	local _,_,r = getElementRotation(localPlayer)
	local px,py = getPointFromDistanceRotation(x, y, 1.5, -r)
	--dxDrawLine3D(x,y,z, px, py,z, tocolor(255,0,0), 3)
	setFocusedElement(findNearestElementOfIntrestAt(px, py, z, getElementInterior(localPlayer), getElementDimension(localPlayer)));
end

addEventHandler("onClientPreRender", root, updateFocusedElement)
