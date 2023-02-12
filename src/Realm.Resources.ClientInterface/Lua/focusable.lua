local focusableElements = {}
local focusableElementsCount = 0;
local focusedElement = nil;
local focusedChildElement = nil;
local debugRenderEnabled = false;

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
		if(focusableElements[v])then
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
	
	if(debugRenderEnabled)then
		dxDrawLine3D(x,y,z, px, py,z, tocolor(255,0,0), 2)
		if(nearestElement)then
			px,py,pz = getElementPosition(nearestElement);
			dxDrawLine3D(x,y,z, px,py,pz, tocolor(0,255,0), 3)
			if(getElementType(nearestElement) == "vehicle" and nearestChildElement)then
				px,py,pz = getVehicleComponentPosition(nearestElement, nearestChildElement, "world")
				dxDrawLine3D(x,y,z, px,py,pz, tocolor(0,0,255), 3)
			end
		end
	end

	setFocusedElement(nearestElement,nearestChildElement);
end

local function internalAddFocusable(element)
	focusableElements[element] = true;
	focusableElementsCount = focusableElementsCount + 1
	if(focusableElementsCount == 1)then
		addEventHandler("onClientPreRender", root, updateFocusedElement)
	end
end

addEvent("internalAddFocusable", true)
addEventHandler("internalAddFocusable", root, function()
	internalAddFocusable(source)
end)

addEvent("internalRemoveFocusable", true)
addEventHandler("internalRemoveFocusable", root, function()
	focusableElements[source] = nil;
	focusableElementsCount = focusableElementsCount + 1
	if(focusableElementsCount == 0)then
		removeEventHandler("onClientPreRender", root, updateFocusedElement)
	end
end)

addEvent("internalAddFocusables", true)
addEventHandler("internalAddFocusables", localPlayer, function(elements)
	for i,v in ipairs(elements)do
		internalAddFocusable(v)
	end
end)

addEvent("internalSetFocusableRenderingEnabled", true)
addEventHandler("internalSetFocusableRenderingEnabled", localPlayer, function(enabled)
	debugRenderEnabled = enabled;
end)
