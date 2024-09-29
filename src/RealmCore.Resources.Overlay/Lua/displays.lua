local displays = {};
local ringd3dMaxDistance = 20;
local lines3d = {};

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

addEventHandler("onClientRender", root, function()
	for id,v in pairs(lines3d)do
		local sx,sy,sz = getPositionFromContext(v.from);
		local ex,ey,ez = getPositionFromContext(v.to);
		local effect = v.effect;
		if(effect.type == "none")then
			dxDrawLine3D(sx,sy,sz, ex,ey,ez, v.color, v.width)
		elseif(effect.type == "gravity")then
			local magnitude = effect.magnitude / 2
			local segments = effect.segments
			local sineAmplitude = 1  -- Amplitude of the sine wave (height of peaks/valleys)

			-- Calculate the direction vector for each segment
			local dx = (ex - sx) / segments
			local dy = (ey - sy) / segments
			local dz = (ez - sz) / segments

			-- Draw each segment
			for i = 0, segments do
				local t = i / segments  -- Normalized position along the line (0 to 1)

				-- Start point for this segment
				local currentX = sx + i * dx
				local currentY = sy + i * dy
				local currentZ = sz + i * dz

				-- Apply sine wave adjustment to the Z-axis based on position `t`
				local sineAdjustment = sineAmplitude * math.sin(t * math.pi) * magnitude

				-- Lower or raise the Z-coordinate according to the sine wave
				currentZ = currentZ - sineAdjustment

				-- Draw the line from the previous point to the current point (starting from the second iteration)
				if i > 0 then
					dxDrawLine3D(prevX, prevY, prevZ, currentX, currentY, currentZ, v.color, v.width)
				end

				-- Store the current point as the previous point for the next iteration
				prevX, prevY, prevZ = currentX, currentY, currentZ
			end
		end
	end

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

function handleCreateLine3d(id, from, to, color, width, effect)
	lines3d[id] = {
		id = id,
		from = from,
		to = to,
		color = color,
		width = width,
		effect = effect
	};
end

function handleRemoveLine3d(ids)
	for i,v in ipairs(ids)do
		lines3d[v] = nil;
	end
end