local assetsList = {}
local loadedAssets = {}
local modelsToReplace = {}
local replacedModels = {}
local downloadingOrDownloadedRemoteAssets = {}

setOcclusionsEnabled(false);

function requestRemoteImageAsset(url)
	if(not downloadingOrDownloadedRemoteAssets[url])then
		fetchRemote(url, function(response)
			loadedAssets[url] = dxCreateTexture(response, "argb", false);
		end)
		downloadingOrDownloadedRemoteAssets[url] = true
	end
	return loadedAssets[url]
end

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
		loadedAssets[name] = assetInfo[3];
	elseif(assetType == "FileSystemFont")then
		checkIfAssetExists(assetInfo[3])
		loadedAssets[name] = dxCreateFont(assetInfo[3], 12)
	elseif(assetType == "RemoteImage")then
		if(not downloadingOrDownloadedRemoteAssets[name])then
			fetchRemote(assetInfo[3], function(response)
				loadedAssets[name] = dxCreateTexture(response, "argb", false);
			end)
			downloadingOrDownloadedRemoteAssets[name] = true
		end
	end

	return loadedAssets[name]
end

addEvent("internalSetAssetsList", true)
addEventHandler("internalSetAssetsList", localPlayer, function(newAssetsList, newModelsToReplace)
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
				if(not colInfo.data)then
					local content = decryptAsset(colInfo[3])
					local col = engineLoadCOL(content)
					colInfo.data = col
				end
				engineReplaceCOL(colInfo.data, modelId)
			end
		end
	end

	do
		local txdInfo = assetsList[modelToReplace.textureAsset]
		if(txdInfo)then
			if(not txdInfo.data)then
				local content = decryptAsset(txdInfo[3])
				local txd = engineLoadTXD(content)
				txdInfo.data = txd
			end
			engineImportTXD(txdInfo.data, modelId)
		end
	end

	do
		local dffInfo = assetsList[modelToReplace.modelAsset]
		if(dffInfo)then
			if(not dffInfo.data)then
				local content = decryptAsset(dffInfo[3])
				local dff = engineLoadDFF(content)
				dffInfo.data = dff
			end
			engineReplaceModel(dffInfo.data, modelId)
		end
	end
end

function tryReplaceModels()
	for model,v in pairs(modelsToReplace)do
		tryReplaceModel(model)
	end
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	for i,v in ipairs(getElementsByType("object", root, true))do
		tryReplaceModel(getElementModel(v));
	end
end)
