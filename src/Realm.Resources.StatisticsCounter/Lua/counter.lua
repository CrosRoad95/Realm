local updateCounterTimer = nil;
local counters = {}
local lastCollectionTick = 0;
local lastX,lastY,lastZ
local totalLastCollectionTraveledDistance = 0;

local function incCounter(name, value)
	counters[name] = (counters[name] or 0) + value
end

local function updateCounter()
	local traveledDistance = getDistanceBetweenPoints3D(lastX, lastY, lastZ, getElementPosition(localPlayer))
	if(traveledDistance > 40 or traveledDistance < 0.1)then
		return; -- Possibly teleported or moved not too enough
	end
	local occupiedSeat = getPedOccupiedVehicleSeat(localPlayer)
	if(occupiedSeat)then
		if(occupiedSeat == 0)then
			incCounter("traveledDistanceInVehicleAsDriver", traveledDistance)
		else
			incCounter("traveledDistanceInVehicleAsPassager", traveledDistance)
		end
	elseif(isElementInWater(localPlayer))then
		incCounter("traveledDistanceSwimming", traveledDistance)
	elseif(isPedOnGround(localPlayer))then
		incCounter("traveledDistanceByFoot", traveledDistance)
	else
		incCounter("traveledDistanceInAir", traveledDistance)
	end
	
	totalLastCollectionTraveledDistance = totalLastCollectionTraveledDistance + traveledDistance
	-- if passed 5 seconds and player traveled at least 25 meters by any way
	if(lastCollectionTick + 5000 < getTickCount() and totalLastCollectionTraveledDistance > 25)then
		triggerServerEvent("internalCollectStatistics", resourceRoot, counters);
		counters = {}
		lastCollectionTick = getTickCount()
		totalLastCollectionTraveledDistance = 0
	end
	lastX, lastY, lastZ = getElementPosition(localPlayer)
end

addEvent("internalSetCounterEnabled", true)
addEventHandler("internalSetCounterEnabled", localPlayer, function(enabled)
	counters = {}
	if(enabled)then
		lastCollectionTick = getTickCount()
		lastX, lastY, lastZ = getElementPosition(localPlayer)
		updateCounterTimer = setTimer(updateCounter, 200, 0)
	else
		killTimer(updateCounterTimer)
		updateCounterTimer = nil
	end
end)
