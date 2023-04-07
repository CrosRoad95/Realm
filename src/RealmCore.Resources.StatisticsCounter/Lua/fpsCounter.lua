local currentFps = 0
local nextTick = 0
local lastFps = {}
local fpsStats = {
    999, -- lowest fps
    0, -- highest fps
}

addEventHandler("onClientPreRender", root, function(deltaTime)
    local now = getTickCount()
    if (now >= nextTick) then
        currentFps = (1 / deltaTime) * 1000
        nextTick = now + 1000
        lastFps[#lastFps + 1] = currentFps;
        fpsStats[1] = math.min(fpsStats[1], currentFps);
        fpsStats[2] = math.max(fpsStats[2], currentFps);
    end
end)

setTimer(function()
    local fpsSum = 0;
    for i,v in ipairs(lastFps)do
        fpsSum = fpsSum + v
    end
    fpsStats[3] = fpsSum / #lastFps
    triggerServerEvent("internalCollectFpsStatistics", resourceRoot, unpack(fpsStats))
    fpsStats = {999,0}
    lastFps = {}
end, 5000, 0)