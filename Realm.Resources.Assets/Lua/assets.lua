local assets = {}
addEvent("onAssetDataReady", false)
local function requestAsset(name)
	if(assets[name])then
		triggerServerEvent("internalRequestAsset", resourceRoot, name)
	end
	return false
end

local function handleRequestChecksums(jsonData)

end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addEvent("internalResponseRequestChecksums", true)
	addEvent("internalResponseRequestAsset", true)
	addEventHandler("internalResponseRequestChecksums", localPlayer, function(json)
		assets = fromJSON(json)
	end)
	addEventHandler("internalResponseRequestAsset", localPlayer, function(name, json)
		assetInfoAndData = fromJSON(json)
		triggerEvent("onAssetDataReady", localPlayer, name, assetInfoAndData["Item1"], assetInfoAndData["Item2"])
	end)

	triggerServerEvent("internalRequestChecksums", resourceRoot)
end)

-- Test code:
addCommandHandler("assetTest", function()
	requestAsset("test")
end)

createObject(1337, -5, 5, 3.2);
addEventHandler("onAssetDataReady", localPlayer, function(name, type, data)
	if(type == "model")then
		local col = engineLoadCOL(base64Decode(data[2]))
		engineReplaceCOL ( col, 1337 )
		local dff = engineLoadDFF (base64Decode(data[1]))
		engineReplaceModel ( dff, 1337 )
	end
end)