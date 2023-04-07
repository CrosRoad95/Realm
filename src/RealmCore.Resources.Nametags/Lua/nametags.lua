-- Settings
local nametagOffset = 1;
local maxDistance = 64;
local nametagScale = 1.4;

local nametags = {}
local white = tocolor(255,255,255,255)
local black = tocolor(0,0,0,255)
local localPlayerRenderingEnabled = false;
local renderingEnabled = false;

addEventHandler("onClientRender", root, function()
	if(not renderingEnabled)then
		return
	end

	local x,y,z, sx,sy, dis, fontScale
	local i,d = getElementInterior(localPlayer), getElementDimension(localPlayer)
	local cx,cy,cz = getCameraMatrix()
	for ped, nametag in pairs(nametags)do
		if(ped and isElement(ped) and getElementInterior(ped) == i and getElementDimension(ped) == d and ((ped ~= localPlayer) or localPlayerRenderingEnabled))then
			x,y,z = getElementPosition(ped)
			dis = getDistanceBetweenPoints3D(cx, cy, cz, x,y,z)
			if(dis < maxDistance)then
				if(isLineOfSightClear(cx,cy,cz, x, y, z, true, true, false, true, true, false, false))then
					sx, sy = getScreenFromWorldPosition(x, y, z + nametagOffset, 64, false)
					if(sx and sy)then
						fontScale = ((maxDistance - dis) / maxDistance) * nametagScale;
						dxDrawText(nametag.text, sx + 2, sy + 2, sx + 2, sy + 2, black, fontScale, "sans", "center", "center", false, false, false, true, true)
						dxDrawText(nametag.text, sx, sy, sx, sy, white, fontScale, "sans", "center", "center", false, false, false, true, true)
					end
				end
			end
		end
	end
end)

function handleAddNametags(allNametags)
	nametags = allNametags
end

function handleSetPedNametag(nametag)
	nametags[source] = nametag
end

function handleRemoveNametag()
	nametags[source] = nametag
end

function handleSetRenderingEnabled(enabled)
	renderingEnabled = enabled
end

function handleSetLocalPlayerRenderingEnabled(enabled)
	localPlayerRenderingEnabled = enabled
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("AddNametags", handleAddNametags)
	hubBind("RemoveNametag", handleRemoveNametag)
	hubBind("SetPedNametag", handleSetPedNametag)
	hubBind("SetRenderingEnabled", handleSetRenderingEnabled)
	hubBind("SetLocalPlayerRenderingEnabled", handleSetLocalPlayerRenderingEnabled)
end)