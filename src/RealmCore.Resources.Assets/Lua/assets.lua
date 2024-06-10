local assetsInfoList = {}
local loadedAssets = {}
local pendingRequestedModels = {}
local modelsToReplace = {}
local replacedModels = {}

function requestAsset(name)
	local assetInfo = assetsInfoList[name];
	if(not assetsInfoList[name])then
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
	checkIfAssetExists(assetInfo[2])
	if(assetType == "MtaFont")then
		loadedAssets[name] = dassetInfo[3];
	elseif(assetType == "FileSystemFont")then
		loadedAssets[name] = dxCreateFont(assetInfo[3], 12)
	end

	return loadedAssets[name]
end

addEvent("internalSetAssetsList", true)
addEventHandler("internalSetAssetsList", localPlayer, function(assetsList, newModelsToReplace)
	-- TODO: Unload models
	for i,v in ipairs({fromJSON(newModelsToReplace)})do
		modelsToReplace[v.model] = v
	end
	assetsInfoList = receivedAssets;
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
	if(replacedModels[modelId] or not modelsToReplace[modelId] or pendingRequestedModels[modelId])then
		return;
	end

	pendingRequestedModels[modelId] = true;
	local modelToReplace = modelsToReplace[modelId];
end

function tryReplaceModels()
	for i,v in ipairs(getElementsByType("object", root, true))do
		tryReplaceModel(getElementModel(v));
	end
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	for i,v in ipairs(getElementsByType("object", root, true))do
		tryReplaceModel(getElementModel(v));
	end
	
	addEventHandler( "onClientElementStreamIn", root, function()
		tryReplaceModel(getElementModel(source))
	end)
end)

createObject(1337, 1000,1000,5)
createObject(1338, 10,0,6)