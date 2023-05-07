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

local function drawEntities()
	local x,y,z;
	local sx,sy, sx2, sy2, tx, ty, delta;
	local camX, camY, camZ = getCameraMatrix()
	local cameraDistance, debugText;
	local fontSize = 1;

	local count = 0;
	for debugId,v in pairs(getEntities())do
		count = count + 1
		if(isElement(v.element))then
			x,y,z = getElementPosition(v.element)
		else
			x,y,z = v.position[1], v.position[2], v.position[3];
		end
		if(v.element ~= localPlayer)then
			cameraDistance = getDistanceBetweenPoints3D(camX, camY, camZ, x,y,z);
			if(getDistanceBetweenPoints3D(camX, camY, camZ, x,y,z) < drawDistance)then
				if(drawPreviewAtPosition(v.previewType, x,y,z, v.color))then
					sx,sy = getScreenFromWorldPosition(x,y,z);
					sx2,sy2 = getScreenFromWorldPosition(x + 1.5,y,z + 1.5);
					if(sx and sy and sx2 and sy2)then
						delta = debugTextScreenMaxDistance / math.max(debugTextScreenMaxDistance, calculateDistanceToScreenCenter(sx2,sy2), 0)
						alpha = getEasingValue(delta, "OutQuad") * 255
						if(cameraDistance < 10)then
							if(isComponentDrawingEnabled())then
								debugText = string.format("Id: %s\nNazwa: %s\n%s", v.debugId, v.name, getComponents(v.debugId))
							else
								debugText = string.format("Id: %s\nNazwa: %s", v.debugId, v.name)
							end
							fontSize = 1.5
						elseif(cameraDistance < 100)then
							debugText = string.format("Id: %s\nNazwa: %s", string.sub(v.debugId, 1, 8), v.name)
							fontSize = 1.0
						elseif(cameraDistance < 250)then
							debugText = string.format("Id: %s\nDystans: %i", string.sub(v.debugId, 1, 8), cameraDistance)
							fontSize = 0.75
						else
							debugText = false
						end
						if(debugText)then
							local colorCoded = false
							if(cameraDistance < 20)then
								tx,ty = dxGetTextSize(debugText, 0, fontSize, 1, "sans", false, colorCoded)
								dxDrawRectangle(sx - 4, sy - 4, 8, 8, v.color)
								if((sx - sx2 - tx/2) > 0)then
									dxDrawLine(sx, sy, sx2 + tx, sy2, v.color, 2, false)
								else
									dxDrawLine(sx, sy, sx2, sy2, v.color, 2, false)
								end
								dxDrawLine(sx2, sy2, sx2 + tx, sy2, v.color, 2, false)
								dxDrawText(debugText, sx2, sy2, sx2, sy2, tocolor(255, 255, 255, alpha), fontSize, "sans", "left", "top", false, false, false, colorCoded)
							else
								dxDrawText(debugText, sx, sy, sx, sy, tocolor(255, 255, 255, alpha), fontSize, "sans", "center", "center", false, false, false, colorCoded)
							end
						end
					end
				end
			end
		end
		end
	--iprint("DRAW",getTickCount(), count)
end

local function enableDrawEntities()
	addEventHandler("onClientRender", root, drawEntities)
end

local function disableDrawEntities()
	removeEventHandler("onClientRender", root, drawEntities)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addTool("Show entities", enableDrawEntities, disableDrawEntities, 0)
end)