local function render()
	dxDrawText("DRAW DEBUG", 0,0,100, 100)
end

addEventHandler("onClientResourceStart", resourceRoot, function()
	addEventHandler("internalOnAdminToolsEnabled", localPlayer, function()
		addEventHandler("onClientRender", root, render)
	end)

	addEventHandler("internalOnAdminToolsDisabled", localPlayer, function()
		removeEventHandler("onClientRender", root, render)
	end)
end)