local screenX, screenY = guiGetScreenSize()
local worldElements = {}
local drawDistance = 300;
local debugTextScreenMaxDistance = 60;

local function isPositionVisible(x,y,z, tolerance)
	local sx, sy, sz = getScreenFromWorldPosition(x,y,z, tolerance or 0)
	return sx and sy
end

local function calculateDistanceToScreenCenter(x,y)
	return getDistanceBetweenPoints2D(x, y, screenX / 2, screenY / 2)
end

local function drawPreviewAtPosition(previewType, x, y, z, color)
	if(not isPositionVisible(x,y,z))then
		return false;
	end
	if(previewType == 0)then -- None
		return true;
	elseif(previewType == 1)then -- Box wireframe
		local boxHalfSize = 0.25;
		local lineWidth = 2

		dxDrawLine3D(x - boxHalfSize,y - boxHalfSize,z - boxHalfSize, x - boxHalfSize,y - boxHalfSize,z + boxHalfSize, color, lineWidth)
		dxDrawLine3D(x + boxHalfSize,y - boxHalfSize,z - boxHalfSize, x + boxHalfSize,y - boxHalfSize,z + boxHalfSize, color, lineWidth)
		dxDrawLine3D(x - boxHalfSize,y + boxHalfSize,z - boxHalfSize, x - boxHalfSize,y + boxHalfSize,z + boxHalfSize, color, lineWidth)
		dxDrawLine3D(x + boxHalfSize,y + boxHalfSize,z - boxHalfSize, x + boxHalfSize,y + boxHalfSize,z + boxHalfSize, color, lineWidth)

		dxDrawLine3D(x - boxHalfSize,y - boxHalfSize,z - boxHalfSize, x + boxHalfSize,y - boxHalfSize,z - boxHalfSize, color, lineWidth)
		dxDrawLine3D(x - boxHalfSize,y - boxHalfSize,z - boxHalfSize, x - boxHalfSize,y + boxHalfSize,z - boxHalfSize, color, lineWidth)
		dxDrawLine3D(x + boxHalfSize,y + boxHalfSize,z - boxHalfSize, x + boxHalfSize,y - boxHalfSize,z - boxHalfSize, color, lineWidth)
		dxDrawLine3D(x + boxHalfSize,y + boxHalfSize,z - boxHalfSize, x - boxHalfSize,y + boxHalfSize,z - boxHalfSize, color, lineWidth)

		dxDrawLine3D(x - boxHalfSize,y - boxHalfSize,z + boxHalfSize, x + boxHalfSize,y - boxHalfSize,z + boxHalfSize, color, lineWidth)
		dxDrawLine3D(x - boxHalfSize,y - boxHalfSize,z + boxHalfSize, x - boxHalfSize,y + boxHalfSize,z + boxHalfSize, color, lineWidth)
		dxDrawLine3D(x + boxHalfSize,y + boxHalfSize,z + boxHalfSize, x + boxHalfSize,y - boxHalfSize,z + boxHalfSize, color, lineWidth)
		dxDrawLine3D(x + boxHalfSize,y + boxHalfSize,z + boxHalfSize, x - boxHalfSize,y + boxHalfSize,z + boxHalfSize, color, lineWidth)
		return true;
	else
		-- Error?
	end
	return false;
end

local function drawWorld()
	local x,y,z;
	local sx,sy, delta;
	local camX, camY, camZ = getCameraMatrix()
	local cameraDistance, debugText;
	local fontSize = 1;
	for debugId,v in pairs(worldElements)do
		if(isElement(v.element))then
			x,y,z = getElementPosition(v.element)
		else
			x,y,z = v.position[1], v.position[2], v.position[3];
		end
		cameraDistance = getDistanceBetweenPoints3D(camX, camY, camZ, x,y,z);
		if(getDistanceBetweenPoints3D(camX, camY, camZ, x,y,z) < drawDistance)then
			if(drawPreviewAtPosition(v.previewType, x,y,z, v.color))then
				sx,sy = getScreenFromWorldPosition(x,y,z);
				delta = debugTextScreenMaxDistance / math.max(debugTextScreenMaxDistance, calculateDistanceToScreenCenter(sx,sy), 0)
				alpha = getEasingValue(delta, "OutQuad") * 255
				if(cameraDistance < 10)then
					debugText = string.format("Id: %s\nTyp: %s\nNazwa: %s", v.debugId, v.type, v.name)
					fontSize = 1.5
				elseif(cameraDistance < 100)then
					debugText = string.format("Id: %s\nTyp: %s\nNazwa: %s", string.sub(v.debugId, 1, 8), v.type, v.name)
					fontSize = 1.0
				elseif(cameraDistance < 250)then
					debugText = string.format("Id: %s\nTyp: %s\nDystans: %i", string.sub(v.debugId, 1, 8), v.type, cameraDistance)
					fontSize = 1.0
				end
				if(debugText)then
					dxDrawText(debugText, sx, sy, sx, sy, tocolor(255, 255, 255, alpha), fontSize, "sans", "center", "center")
				end
			end
		end
	end
end

local function enableDrawWorld()
	triggerServerEvent("internalDrawWorldSubscribe", resourceRoot)
	addEventHandler("onClientRender", root, drawWorld)
end

local function disableDrawWorld()
	triggerServerEvent("internalDrawWorldUnsubscribe", resourceRoot)
	removeEventHandler("onClientRender", root, drawWorld)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addTool("Debug world", enableDrawWorld, disableDrawWorld)
end)

function handleInternalAddOrUpdateDebugElements(debugData)
	for i,v in ipairs(debugData)do
		v.color = tocolor(v.color[1], v.color[2], v.color[3], v.color[4])
		worldElements[v.debugId] = v
	end
	--print("updated or added:", #debugData, "elements");
end