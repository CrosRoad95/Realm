local screenX, screenY = guiGetScreenSize()
local worldElements = {}
local drawDistance = 150;
local debugTextScreenMaxDistance = 10;
local color = tocolor(0,255,255)
local nameColor = tocolor(255,255,255)

local function isPositionVisible(x,y,z, tolerance)
	local sx, sy = getScreenFromWorldPosition(x,y,z, tolerance or 0)
	return sx and sy
end

local function calculateDistanceToScreenCenter(x,y)
	return getDistanceBetweenPoints2D(x, y, screenX / 2, screenY / 2)
end

function getPointFromDistanceRotation(x, y, dist, angle)
    local a = math.rad(90 - angle);
    local dx = math.cos(a) * dist;
    local dy = math.sin(a) * dist;
    return x+dx, y+dy;
end

local function render()
	local cameraDistance;
	local camX, camY, camZ = getCameraMatrix()
	for i,v in ipairs(getSpawnMarkers())do
		if(v.type == "point")then
			cameraDistance = getDistanceBetweenPoints3D(v.position[1],v.position[2],v.position[3], camX, camY, camZ);
			if(drawDistance > cameraDistance)then
				local sx, sy = getScreenFromWorldPosition(v.position[1],v.position[2],v.position[3], tolerance or 0)
				if(sx and sy)then
					dxDrawRectangle(sx - 3, sy - 3, 6, 6, color)
					if(cameraDistance < debugTextScreenMaxDistance)then
						dxDrawText("- "..v.name, sx + 10, sy, sx + 200, sy, nameColor, 1, 1, "default", "left", "center")
					end
				end
			end
		elseif(v.type == "directional")then
			cameraDistance = getDistanceBetweenPoints3D(v.position[1],v.position[2],v.position[3], camX, camY, camZ);
			if(drawDistance > cameraDistance)then
				local sx, sy = getScreenFromWorldPosition(v.position[1],v.position[2],v.position[3], tolerance or 0)
				if(sx and sy)then
					local px, py = getPointFromDistanceRotation(v.position[1], v.position[2], 1, v.direction)
					dxDrawRectangle(sx - 3, sy - 3, 6, 6, color)
					if(cameraDistance < debugTextScreenMaxDistance)then
						dxDrawLine3D(v.position[1], v.position[2], v.position[3], px, py, v.position[3], color, 0.25, false)
						dxDrawText("- "..v.name, sx + 10, sy, sx + 200, sy, nameColor, 1, 1, "default", "left", "center")
					end
				end
			end
		end
	end
end

local function enableDrawShowSpawnMarkers()
	addEventHandler("onClientRender", root, render)
end

local function disableDrawShowSpawnMarkers()
	removeEventHandler("onClientRender", root, render)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addTool("Show spawn markers", enableDrawShowSpawnMarkers, disableDrawShowSpawnMarkers, 2)
end)