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

local function getFocusPosition(element)
	local ex, ey, ez = getElementPosition(element);
	if(getElementType(element) == "object")then
		local x0, y0, z0, x1, y1, z1 = getElementBoundingBox(element)
		ex, ey, ez = ex + (x0 + x1) / 2, ey + (y0 + y1) / 2, ez + (z0 + z1) / 2
	else
		ex, ey, ez = getElementPosition(element);
	end
	return ex, ey, ez;
end

local function findNearestElementToPoint(elements, x,y,z)
	local nearestElement = nil;
	local nearestComponent = nil;
	local nearest = 9999999
	local dis = 0;
	local ex,ey,ez;
	for i,v in ipairs(elements)do
		if(focusableElements[v])then
			if(v ~= localPlayer)then
				ex, ey, ez = getFocusPosition(v)
				dis = getDistanceBetweenPoints3D(x,y,z, ex, ey, ez)
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

local function drawBox(x0, y0, z0, x1, y1, z1)
	dxDrawLine3D(x0, y0, z0, x1, y1, z1, tocolor(0,0,255), 1)
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
	local px,py,pz = getPointFromDistanceRotation(x, y, 0.5, -r)
	local nearestElement,nearestChildElement = findNearestElementOfIntrestAt(px, py, z, getElementInterior(localPlayer), getElementDimension(localPlayer))
	
	if(debugRenderEnabled)then
		for focusableElement in pairs(focusableElements)do
			local ex,ey,ez = getElementPosition(focusableElement)
			dxDrawLine3D(ex,ey,ez + 3, ex,ey,ez + 1, tocolor(0,0,255), 4)
		end
		dxDrawLine3D(x,y,z, px, py,z, tocolor(255,0,0), 2)
		if(nearestElement and isElement(nearestElement))then
			px,py,pz = getFocusPosition(nearestElement);
			dxDrawLine3D(x,y,z, px,py,pz, tocolor(0,255,0), 3)
			if(getElementType(nearestElement) == "vehicle" and nearestChildElement)then
				cx,cy,cz = getVehicleComponentPosition(nearestElement, nearestChildElement, "world")
				dxDrawLine3D(x,y,z, cx,cy,cz, tocolor(0,0,255), 3)
			elseif(getElementType(nearestElement) == "object")then
				local x0, y0, z0, x1, y1, z1 = getElementBoundingBox(nearestElement)
				drawBox(px + x0, py + y0, pz + z0, px + x1, py + y1, pz + z1)
			end
		end
	end

	setFocusedElement(nearestElement,nearestChildElement);
end

local function internalAddFocusable(element)
	if(not focusableElements[element])then
		focusableElements[element] = true;
		focusableElementsCount = focusableElementsCount + 1
		if(focusableElementsCount == 1)then
			addEventHandler("onClientPreRender", root, updateFocusedElement)
		end
	end
end

local function internalRemoveFocusable(element)
	if(focusableElements[element])then
		focusableElements[element] = nil;
		focusableElementsCount = focusableElementsCount - 1
		if(focusableElementsCount == 0)then
			removeEventHandler("onClientPreRender", root, updateFocusedElement)
		end
	end
end

local function handleAddFocusable()
	internalAddFocusable(source)
end

local function handleRemoveFocusable()
	internalRemoveFocusable(source)
end

local function handleAddFocusables(focusables)
	for i,v in ipairs(focusables)do
		internalAddFocusable(v)
	end
end

local function handleAetFocusableRenderingEnabled(enabled)
	debugRenderEnabled = enabled;
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("AddFocusable", handleAddFocusable)
	hubBind("RemoveFocusable", handleRemoveFocusable)
	hubBind("AddFocusables", handleAddFocusables)
	hubBind("SetFocusableRenderingEnabled", handleAetFocusableRenderingEnabled)
end)
