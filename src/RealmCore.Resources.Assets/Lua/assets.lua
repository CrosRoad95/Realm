local assetInfos = {}
local loadedAssets = {}

function requestAsset(name)
	local assetInfo = assetInfos[name];
	if(not assetInfos[name])then
		error("Could not find asset: '"..name.."'")
	end
	if(not loadedAssets[name])then
		loadAsset(name, assetInfo)
	end

	return loadedAssets[name]
end

function loadAsset(name, assetInfo)
	local assetType = assetInfo[1]
	if(assetType == "Font")then
		loadedAssets[name] = dxCreateFont(assetInfo[2], 12)
		outputDebugString("Loaded font: "..tostring(name))
	end

	return loadedAssets[name]
end

addEvent("internalResponseRequestAsset", true)
addEventHandler("internalResponseRequestAsset", localPlayer, function(receivedAssets)
	assetInfos = receivedAssets
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

triggerServerEvent("internalRequestAssets", resourceRoot)
