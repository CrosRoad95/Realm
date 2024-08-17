local visibleHuds = {}
local huds = {}
local huds3d = {}
local assets = {}
local hud3dResolution = 128; -- 128 pixels per 1m
local visibleCounter = 0;
local handleRenderHuds;
local fps = 0;
local counter = 0;
local startTick;
local currentTick;

local function isHudVisible(hudId)
	return visibleHuds[hudId] and true or false
end

local function setHudVisibleCore(hudId, visible)
	if(isHudVisible(hudId) ~= visible)then
		if(visible)then
			visibleCounter = visibleCounter + 1;
			visibleHuds[hudId] = true;
		else
			visibleCounter = visibleCounter - 1;
			visibleHuds[hudId] = nil;
		end
	else
		return false;
	end

	if(visible)then
		if(visibleCounter == 1)then
			addEventHandler("onClientRender", root, handleRenderHuds);
		end
	else
		if(visibleCounter == 0)then
			removeEventHandler("onClientRender", root, handleRenderHuds);
		end
	end
end

local function getElementSpeed(theElement, unit)
    local elementType = getElementType(theElement)
    unit = unit == nil and 0 or ((not tonumber(unit)) and unit or tonumber(unit))
    local mult = (unit == 0 or unit == "m/s") and 50 or ((unit == 1 or unit == "km/h") and 180 or 111.84681456)
    return (Vector3(getElementVelocity(theElement)) * mult).length
end

local function calculateBoundingBox(position, elements)
	local x,y = unpack(position)
	local maxX, maxY = 0,0
	for i,v in ipairs(elements)do
		maxX = math.max(maxX, v[4] + x, v[4] + v[6] + x)
		maxY = math.max(maxY, v[5] + y, v[5] + v[7] + y)
	end
	return maxX + x, maxY + y
end

local function renderHud(position, elements)
	local x,y = unpack(position);
	local ex, ey, position;
	local content;
	for i,element in ipairs(elements)do
		ex, ey = element.position[1], element.position[2];
		if(element.positioningMode == "Relative")then
			ex = ex + x;
			ey = ey + y;
		end
		if(element.type == "text")then
			content = element.content;
			local text = nil;
			if(content.type == "constant")then
				text = content.value;
			elseif(content.type == "computed")then
				if(content.value == "vehicleSpeed")then
					local vehicle = getPedOccupiedVehicle(localPlayer);
					if(vehicle and getVehicleController(vehicle) == localPlayer)then
						local speed = getElementSpeed(vehicle, "km/s");
						text = string.format("%ikm/h", speed);
					end
				elseif(content.value == "fps")then
					text = string.format("FPS: %i", fps);
				end
			end

			if(text)then
				dxDrawText(text, ex, ey, ex + element.size[1], ey + element.size[2], element.color, element.scale[1], element.scale[2], element.font or "sans", element.align[1], element.align[2])
			end
		elseif(element.type == "rectangle")then
			dxDrawRectangle(ex, ey, element.size[1], element.size[2], element.color)
		elseif(element.type == "radar")then
			if(element.data == nil)then
			end
			if(element.map == nil)then
				element.map = prepareAsset(element.image)
			end
			if(element.map ~= nil and element.data == nil)then
				element.data = radarCreateInstance(ex, ey, element.size[1], element.size[2], element.map)
			end
			
			if(element.data ~= nil)then
				radarRender(element.data);
			end

			--dxDrawImage(ex, ey, v[5], v[6], v.data.rt, 0, 0, 0)
		end
	end
end

function prepareAsset(assetInfo)
	iprint("prepare asset", assetInfo)
	local assetType = assetInfo[1];
	if(assetType == "FileSystemFont")then
		iprint("fuile system font", assetInfo)
		if(not assets[assetInfo[2]])then
			return requestAsset(assetInfo[2])
		end
	elseif(assetType == "MtaFont")then
		return assetInfo[2]
	elseif(assetType == "RemoteImage")then
		return requestAsset(assetInfo[2])
	end
	return assetInfo;
end

local function prepareElements(elements)
	for i,v in ipairs(elements)do
		if(v.type == "text" or v.type == "computedValue")then
			v.font = prepareAsset(v.font);
		elseif(v.type == "radar")then
			v.map = prepareAsset(v.image);
		end
	end
end

local function renderHud3d(elements, oldrt)
	local rt;
	prepareElements(elements);
	local sx,sy = calculateBoundingBox({0, 0}, elements)
	if(oldrt)then
		local osx, osy = dxGetMaterialSize(oldrt);
		if(osx == sx and osy == sy)then
			rt = oldrt
		else
			destroyElement(oldrt)
			rt = dxCreateRenderTarget(sx, sy, false)
		end
	else
		rt = dxCreateRenderTarget(sx, sy, false)
	end
    dxSetRenderTarget(rt)
	renderHud({0,0}, elements)
    dxSetRenderTarget()
	return rt, sx, sy;
end

function handleRenderHuds()
	if(isPlayerMapVisible())then
		return;
	end

	for hudId,hud in pairs(huds)do
		if(isHudVisible(hudId))then
			renderHud(hud.position, hud.elements)
		end
	end
end

local function renderHuds3d()
	local h;
	for i,v in pairs(huds3d)do
		h = v.size[2] / hud3dResolution;
		if(v.dirtyState or not isElement(v.element))then
			local newrt, sx, sy = renderHud3d(v.elements, v.element)
			v.element = newrt;
			v.size = {sx, sy}
			v.dirtyState = false;
		end
		dxDrawMaterialLine3D(v.position[1], v.position[2], v.position[3] + h/2, v.position[1], v.position[2], v.position[3] - h/2, false, v.element, v.size[1] / hud3dResolution)
	end
end

local function handleAddNotification(message)
	notifications[#notifications + 1] = {
		visibleUntil = getTickCount() + 5000,
		message = message,
	}
end

local function setHudStateCore(elements, newState)
	for i,element in ipairs(elements)do
		if(newState[element.id])then
			if(element.type == "text")then
				element.content.value = newState[element.id];
			end
		end
	end
end

local function handleSetHudVisible(hudId, enabled)
	if(huds[hudId])then
		setHudVisibleCore(hudId, enabled);
	else
		outputDebugString("Failed to setHudVisible, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleSetHudPosition(hudId, x, y)
	if(huds[hudId])then
		huds[hudId].position = {x, y};
	else
		outputDebugString("Failed to setHudPosition, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleSetHudState(hudId, newHudState)
	if(huds[hudId])then
		setHudStateCore(huds[hudId].elements, newHudState)
	else
		outputDebugString("Failed to setHudState, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleSetHud3dState(hudId, newHudState)
	if(huds3d[hudId])then
		setHudStateCore(huds3d[hudId].elements, newHudState)
		huds3d[hudId].dirtyState = true;
	else
		outputDebugString("Failed to setHud3dState, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleCreateHud(hudId, x, y, elements)
	if(huds[hudId])then
		outputDebugString("Failed to createHud, hud of id: '"..tostring(hudId).."' already exists.", 1);
		return;
	end

	huds[hudId] = {
		position = {x,y},
		elements = elements,
	};
	prepareElements(elements);
end

local function handleCreateHud3d(hudId, x, y, z, elements)
	if(huds3d[hudId])then
		outputDebugString("Failed to createHud3d, hud of id: '"..tostring(hudId).."' already exists.", 1);
		return;
	end

	huds3d[hudId] = {
		elements = elements,
		position = {x,y,z},
		size = {-1, -1},
		element = nil,
		dirtyState = true,
	}
end

local function handleRemoveHud(hudId)
	if(huds[hudId])then
		huds[hudId] = nil;
		setHudVisibleCore(hudId, false);
	else
		outputDebugString("Failed to removeHud, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

local function handleRemoveHud3d(hudId)
	if(huds3d[hudId])then
		destroyElement(huds3d[hudId].element)
		huds3d[hudId] = nil;
		setHudVisibleCore(hudId, false);
	else
		outputDebugString("Failed to removeHud3d, hud of id: '"..tostring(hudId).."' not found.", 1);
	end
end

addEventHandler("onClientPreRender", root, function()
        if not startTick then
            startTick = getTickCount()
        end
        counter = counter + 1;
        currentTick = getTickCount();
        if currentTick - startTick >= 1000 then 
			fps = counter;
            counter = 0;
            startTick = false;
        end 
end);

addEventHandler("onClientResourceStart", resourceRoot, function()
	hubBind("AddNotification", handleAddNotification)
	hubBind("SetHudVisible", handleSetHudVisible)
	hubBind("SetHudPosition", handleSetHudPosition)
	hubBind("SetHudState", handleSetHudState)
	hubBind("SetHud3dState", handleSetHud3dState)
	hubBind("CreateHud", handleCreateHud)
	hubBind("CreateHud3d", handleCreateHud3d)
	hubBind("RemoveHud", handleRemoveHud)
	hubBind("RemoveHud3d", handleRemoveHud3d)
	hubBind("AddDisplay3dRing", handleAddDisplay3dRing)
	hubBind("RemoveDisplay3dRing", handleRemoveDisplay3dRing)
	
	addEventHandler("onClientRender", root, renderHuds3d); -- TODO:
end)

--------
-- Radar
--------

createBlip(-2398.69385, -580.74835, 132.74271, 43)
createBlip(-2667.43286, -243.74574, 6.23506, 43)
createBlip(0, 0, 0, 43)

function radarCreateInstance(px, py, psx, psy, mapTexture, blipsTextures)
	local data = {
		radarPosition = {px, py},
		radarSize = {psx, psy},
		size = {3072, 3072},
		blipsScale = 32,
		rotationEnabled = true,
		mapTexture = mapTexture,
		blipsTextures = blipsTextures,
		radarShader = dxCreateShader(embeddedAssets.radar),
		cameraRotation = 0,
		zoom = 1,
		needsRefresh = true
	}
	data.renderTarget = dxCreateRenderTarget(data.size[1], data.size[2], true);
    dxSetShaderValue(data.radarShader, "sPicTexture", data.renderTarget);
	return data;
end

function radarRender(data)
	if(data.needsRefresh)then
		dxSetRenderTarget(data.renderTarget, true);
		dxDrawImage(0, 0, data.size[1], data.size[2], data.mapTexture);
		dxSetRenderTarget();
		data.needsRefresh = false;
	end

    local px,py,pz = getElementPosition(localPlayer)
	local x = (px) / 6000
	local y = (py) / -6000
	dxSetShaderValue(data.radarShader, "gUVPosition", x, y)
	dxSetShaderValue(data.radarShader, "gUVScale", ( data.radarSize[1] / data.zoom / data.size[1] ), ( data.radarSize[2] / data.zoom / data.size[2]  ))

    if(data.rotationEnabled)then
        _, _, tempCameraRotation = getElementRotation(getCamera())
		data.cameraRotation = tempCameraRotation;
        dxSetShaderValue(data.radarShader, "gUVRotAngle", math.rad(-data.cameraRotation))
    else
        data.cameraRotation = 0;
        dxSetShaderValue(data.radarShader, "gUVRotAngle", 0)
    end

    dxDrawImage(data.radarPosition[1], data.radarPosition[2], data.radarSize[1], data.radarSize[2], data.radarShader)
    radarRenderBlips(data, px, py, pz, data.blipsTextures);
end

function radarRenderBlips(data, px, py, pz, blipsTextures)
    for k,blip in pairs(getElementsByType("blip")) do
        radarRenderBlip(data, blip, px,py,pz, blipsTextures);
    end
end

function radarRenderBlip(data, blip, px, py, pz, blips)
    local attach = getElementAttachedTo(blip)
    local bx, by, bz = getElementPosition(blip)
    local dist = getDistanceBetweenPoints2D(px, py, bx, by) / 2 * data.zoom
    local rot = math.atan2(bx - px, by - py) + math.rad(data.cameraRotation)
    local icon = getBlipIcon(blip);
	local blipsScale = data.blipsScale;

	local x, y = radarGetPositionFromWorld(data, bx, by, rot, dist, blipsScale)
    if fileExists("blips/"..icon..".png") and getBlipVisibleDistance(blip) >= dist then
        dxDrawImage(x, y - blipsScale, blipsScale, blipsScale, "blips/"..icon..".png", 0, 0, 0, tocolor(255, 255, 255, 200))
    else
		dxDrawRectangle(x, y - blipsScale, blipsScale, blipsScale, tocolor(128,0,128));
	end
end

function radarGetPositionFromWorld(data, x, y, rotation, dist, blipsScale)
    local w, h = data.radarSize[1], data.radarSize[2]
	local rx,ry = data.radarPosition[1], data.radarPosition[2];
    local x = rx + w / 2 + math.sin(rotation) * dist - blipsScale / 2
    local y = ry + h / 2 - math.cos(rotation) * dist + blipsScale / 2

    x = math.min(math.max(x, rx + 2 - blipsScale / 2), rx + w - blipsScale / 2)
    y = math.min(math.max(y, ry + blipsScale + 3 - blipsScale / 2), ry + h - 4 + blipsScale / 2)
    return x, y
end

------
-- Embedded assets
------

embeddedAssets = {
	radar = [[float3x3 makeTranslationMatrix ( float2 pos )
{
    return float3x3(
                    1, 0, 0,
                    0, 1, 0,
                    pos.x, pos.y, 1
                    );
}

float3x3 makeRotationMatrix ( float angle )
{
    float s = sin(angle);
    float c = cos(angle);
    return float3x3(
                    c, s, 0,
                    -s, c, 0,
                    0, 0, 1
                    );
}

float3x3 makeScaleMatrix ( float2 scale )
{
    return float3x3(
                    scale.x, 0, 0,
                    0, scale.y, 0,
                    0, 0, 1
                    );
}

float3x3 makeTextureTransform ( float2 prePosition, float2 scale, float2 scaleCenter, float rotAngle, float2 rotCenter, float2 postPosition )
{
    float3x3 matPrePosition = makeTranslationMatrix( prePosition );
    float3x3 matToScaleCen = makeTranslationMatrix( -scaleCenter );
    float3x3 matScale = makeScaleMatrix( scale );
    float3x3 matFromScaleCen = makeTranslationMatrix( scaleCenter );
    float3x3 matToRotCen = makeTranslationMatrix( -rotCenter );
    float3x3 matRot = makeRotationMatrix( rotAngle );
    float3x3 matFromRotCen = makeTranslationMatrix( rotCenter );
    float3x3 matPostPosition = makeTranslationMatrix( postPosition );

    float3x3 result =
                    mul(
                    mul(
                    mul(
                    mul(
                    mul(
                    mul(
                    mul(
                        matPrePosition
                        ,matToScaleCen)
                        ,matScale)
                        ,matFromScaleCen)
                        ,matToRotCen)
                        ,matRot)
                        ,matFromRotCen)
                        ,matPostPosition)
                    ;
    return result;
}

texture sPicTexture;
texture sMaskTexture;

float2 gUVPrePosition = float2( 0, 0 );
float2 gUVScale = float( 1 );                     // UV scale
float2 gUVScaleCenter = float2( 0.5, 0.5 );
float gUVRotAngle = float( 0 );                   // UV Rotation
float2 gUVRotCenter = float2( 0.5, 0.5 );
float2 gUVPosition = float2( 0, 0 );              // UV position

float3x3 getTextureTransform()
{
    return makeTextureTransform( gUVPrePosition, gUVScale, gUVScaleCenter, gUVRotAngle, gUVRotCenter, gUVPosition );
}

technique hello
{
    pass P0
    {
        // Set up texture stage 0
        Texture[0] = sPicTexture;
        TextureTransform[0] = getTextureTransform();
        TextureTransformFlags[0] = Count2;
        AddressU[0] = Clamp;
        AddressV[0] = Clamp;
        // Color mix texture and diffuse
        ColorOp[0] = Modulate;
        ColorArg1[0] = Texture;
        ColorArg2[0] = Diffuse;
        // Alpha mix texture and diffuse
        AlphaOp[0] = Modulate;
        AlphaArg1[0] = Texture;
        AlphaArg2[0] = Diffuse;
     

        // Set up texture stage 1
        Texture[1] = sMaskTexture;
        TexCoordIndex[1] = 0;
        AddressU[1] = Clamp;
        AddressV[1] = Clamp;
        // Color pass through from stage 0
        ColorOp[1] = SelectArg1;
        ColorArg1[1] = Current;
        // Alpha modulate mask texture with stage 0
        AlphaOp[1] = Modulate;
        AlphaArg1[1] = Current;
        AlphaArg2[1] = Texture;


        // Disable texture stage 2
        ColorOp[2] = Disable;
        AlphaOp[2] = Disable;
    }
}]]
}