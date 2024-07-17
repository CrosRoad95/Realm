local assetsList = {}
local loadedAssets = {}
local modelsToReplace = {}
local replacedModels = {}

function requestAsset(name)
	local assetInfo = assetsList[name];
	if(not assetsList[name])then
		error("Could not find asset: '"..name.."'")
	end
	if(not loadedAssets[name])then
		loadAsset(name, assetInfo)
	end
	return loadedAssets[name]
end

local function checkIfAssetExists(path)
	if(not fileExists(path))then
		error("Asset file: ''"..path.."'' doesn't exists'");
	end
end

function loadAsset(name, assetInfo)
	local assetType = assetInfo[1]
	if(assetType == "MtaFont")then
		loadedAssets[name] = dassetInfo[3];
	elseif(assetType == "FileSystemFont")then
		checkIfAssetExists(assetInfo[3])
		loadedAssets[name] = dxCreateFont(assetInfo[3], 12)
	end

	return loadedAssets[name]
end

addEvent("internalSetAssetsList", true)
addEventHandler("internalSetAssetsList", localPlayer, function(newAssetsList, newModelsToReplace)
	--iprint("assetsList",newAssetsList)
	--iprint("newModelsToReplace",newModelsToReplace)
	-- TODO: Unload models
	for i,v in ipairs({fromJSON(newModelsToReplace)})do
		modelsToReplace[v.model] = v
	end
	assetsList = newAssetsList;
	tryReplaceModels();
end)

addEvent("internalReplaceModel", true)
addEventHandler("internalReplaceModel", localPlayer, function(dffData, colData, model)
	local col = engineLoadCOL ( base64Decode(colData))
	engineReplaceCOL ( col, model )
	local dff = engineLoadDFF (base64Decode(dffData) )
	engineReplaceModel ( dff, model )
end)

addEvent("internalRestoreModel", true)
addEventHandler("internalRestoreModel", localPlayer, function(model)
	engineRestoreCOL(model)
	engineRestoreModel(model)
end)

function tryReplaceModel(modelId)
	if(replacedModels[modelId] or not modelsToReplace[modelId])then
		return;
	end

	replacedModels[modelId] = true;

	local modelToReplace = modelsToReplace[modelId];
	
	do
		if(modelToReplace.collisionAsset)then
			local colInfo = assetsList[modelToReplace.collisionAsset]
			if(colInfo)then
				if(colInfo[4])then
					engineReplaceCOL(colInfo[4], modelId)
				else
					local content = decryptAsset(colInfo[3])
					local col = engineLoadCOL(content)
					engineReplaceCOL(col, modelId)
					colInfo[4] = col
				end
			end
		end
	end

	do
		local txdInfo = assetsList[modelToReplace.textureAsset]
		if(txdInfo)then
			if(txdInfo[4])then
				engineImportTXD(txdInfo[4], modelId)
			else
				local content = decryptAsset(txdInfo[3])
				local txd = engineLoadTXD(content)
				engineImportTXD(txd, modelId)
				txdInfo[4] = dff
			end
		end
	end

	do
		local dffInfo = assetsList[modelToReplace.modelAsset]
		if(dffInfo)then
			if(dffInfo[4])then
				engineReplaceModel(dffInfo[4], modelId)
			else
				local content = decryptAsset(dffInfo[3])
				local dff = engineLoadDFF(content)
				engineReplaceModel(dff, modelId)
				dffInfo[4] = dff
			end
		end
	end
end

function tryReplaceModels()
	--for i,v in ipairs(getElementsByType("object", root, true))do
	--	tryReplaceModel(getElementModel(v));
	--end

	for model,v in pairs(modelsToReplace)do
		tryReplaceModel(model)
	end
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	for i,v in ipairs(getElementsByType("object", root, true))do
		tryReplaceModel(getElementModel(v));
	end
	
	--addEventHandler( "onClientElementStreamIn", root, function()
	--	tryReplaceModel(getElementModel(source))
	--end)
end)
