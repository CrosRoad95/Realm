-- Based on: https://community.multitheftauto.com/index.php?p=resources&s=details&id=344
local scopedNames = {}
local globalNames = {}
local visibleCategories = {[0] = true} -- Make category 0 visible by default

-- Settings variables
local textFont       = "default-bold"			-- The font of the tag text
local textScale      = 1						-- The scale of the tag text
local heightPadding  = 1						-- The amount of pixels the tag should be extended on either side of the vertical axis
local widthPadding   = 1						-- The amount of pixels the tag should be extended on either side of the horizontal axis
local xOffset        = 8						-- Distance between the player blip and the tag
local minAlpha       = 10						-- If blip alpha falls below this, the tag won't the shown
local textAlpha      = 255
local rectangleColor = tocolor(0,0,0,230)
local w,h            = guiGetScreenSize()

local sx,sy,ex,ey								-- Map positions
local mw,mh										-- Map width/height
local cx,cy										-- Center position of the map
local ppuX,ppuY									-- Pixels per unit
local fontHeight								-- Height of the specified font
local yOffset									-- How much pixels the tag should be offsetted at

local function renderMapName(id, mapName)
	if(getElementInterior(localPlayer) ~= mapName.interior or getElementDimension(localPlayer) ~= mapName.dimension)then
		return;
	end

	if(not visibleCategories[mapName.category])then
		return;
	end

	local px,py;
	if(mapName.attachedTo)then
		if(not isElement(mapName.attachedTo))then
			return; 
		end
		px, py = getElementPosition(mapName.attachedTo);
	else
		px,py = mapName.position[1], mapName.position[2];
	end

	local x = math.floor(cx+px*ppuX+xOffset);
	local y = math.floor(cy+py*ppuY-yOffset);
	local pname = mapName.name;
	local nameLength = dxGetTextWidth(pname,textScale,textFont);
	local r,g,b,a = mapName.color.R, mapName.color.G, mapName.color.B, mapName.color.A;
				
	if a>minAlpha then
		dxDrawRectangle(x-widthPadding,y+heightPadding,nameLength+widthPadding*2,fontHeight-heightPadding*2,rectangleColor,false);
		dxDrawText(pname,x,y,x+nameLength,y+fontHeight,tocolor(r,g,b,textAlpha),textScale,textFont,"left","top",false,false,false);
	end
end

local function drawMapStuff()
	if not isPlayerMapVisible() then
		return;
	end
		
	sx,sy,ex,ey = getPlayerMapBoundingBox();
	mw,mh = ex-sx,sy-ey;
	cx,cy = (sx+ex)/2,(sy+ey)/2;
	ppuX,ppuY = mw/6000,mh/6000;
	fontHeight = dxGetFontHeight(textScale,textFont);
	yOffset = fontHeight/2;
		
	for k,v in pairs(globalNames) do
		renderMapName(k, v);
	end
	for k,v in pairs(scopedNames) do
		renderMapName(k, v);
	end
end

local function handleAdd(id, name, color, x,y,z, dimension, interior, attachedTo, category, scoped)
	local mapName = {
		name = name,
		color = color,
		position = {x,y,z},
		dimension = dimension,
		interior = interior,
		attachedTo = attachedTo,
		category = category
	};
	if(scoped)then
		scopedNames[id] = mapName;
	else
		globalNames[id] = mapName;
	end
end

local function handleRemove(id)
	scopedNames[id] = nil;
	globalNames[id] = nil;
end

local function handleSetVisibleCategories(categories)
	visibleCategories = {};

	for i,v in ipairs(categories)do
		visibleCategories[v] = true;
	end
end

local function handleSetName(id, name)
	if(scopedNames[id])then
		scopedNames[id].name = name;
	end

	if(globalNames[id])then
		globalNames[id].name = name;
	end
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addEventHandler("onClientRender",getRootElement(),drawMapStuff)

	hubBind("Add", handleAdd);
	hubBind("Remove", handleRemove);
	hubBind("SetVisibleCategories", handleSetVisibleCategories);
	hubBind("SetName", handleSetName);
end)
