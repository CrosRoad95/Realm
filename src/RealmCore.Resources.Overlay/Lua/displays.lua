local displays = {}

local function dxDrawRing (posX, posY, radius, width, startAngle, amount, color, postGUI, absoluteAmount, anglesPerLine)
	if (type (posX) ~= "number") or (type (posY) ~= "number") or (type (startAngle) ~= "number") or (type (amount) ~= "number") then
		return false
	end
	
	if absoluteAmount then
		stopAngle = amount + startAngle
	else
		stopAngle = (amount * 360) + startAngle
	end
	
	anglesPerLine = type (anglesPerLine) == "number" and anglesPerLine or 1
	radius = type (radius) == "number" and radius or 50
	width = type (width) == "number" and width or 5
	color = color or tocolor (255, 255, 255, 255)
	postGUI = type (postGUI) == "boolean" and postGUI or false
	absoluteAmount = type (absoluteAmount) == "boolean" and absoluteAmount or false
	
	for i = startAngle, stopAngle, anglesPerLine do
		local startX = math.cos (math.rad (i)) * (radius - width)
		local startY = math.sin (math.rad (i)) * (radius - width)
		local endX = math.cos (math.rad (i)) * (radius + width)
		local endY = math.sin (math.rad (i)) * (radius + width)
		dxDrawLine (startX + posX, startY + posY, endX + posX, endY + posY, color, width, postGUI)
	end
	return math.floor ((stopAngle - startAngle)/anglesPerLine)
end

local ringd3dMaxDistance = 20;
addEventHandler("onClientRender", root, function()
	local displaysToRemove = {}
	for id,v in pairs(displays)do
		if(v.start + v.time < getTickCount())then
			displaysToRemove[#displaysToRemove + 1] = id
		else
			if(v.type == "ring3d")then
				local dis = getDistanceBetweenPoints3D(v.position[1], v.position[2], v.position[3], getCameraMatrix())
				if(dis < ringd3dMaxDistance)then
					local sx,sy = getScreenFromWorldPosition(v.position[1], v.position[2], v.position[3], 100, false)
					if(sx and sy )then
						local radius = (ringd3dMaxDistance - dis) * 2.5;
						dxDrawRing(sx, sy, radius, radius / 4, 0, math.min(1, (getTickCount() - v.start) / v.time), tocolor(255,255,255,255))
					end
				end
			end
		end
	end

	for _,displayId in ipairs(displaysToRemove)do
		displays[displayId] = nil;
	end
end)

function handleAddDisplay3dRing(id, x,y,z, time)
	if(displays[id])then
		outputDebugString("Failed to addDisplay3dRing of id: '"..tostring(hudId).."' already exists.", 1);
	else
		displays[id] = {
			type = "ring3d",
			position = {x,y,z},
			start = getTickCount(),
			time = time,
		}
	end
end

function handleRemoveDisplay3dRing(id)
	if(displays[id])then
		displays[id] = nil
	else
		outputDebugString("Failed to removeDisplay3dRing of id: '"..tostring(hudId).."' not found.", 1);
	end
end