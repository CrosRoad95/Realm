local debugMessagesBuffer = {};
local flushDebugMessagesBufferTimer = nil;

local function flushDebugMessagesBuffer()
	flushDebugMessagesBufferTimer = nil;
	triggerServerEventWithId("sendDebugMessagesBuffer", debugMessagesBuffer);
	debugMessagesBuffer = {};
end

addEventHandler("onClientDebugMessage", root, function(message, level, file, line)
	if(not flushDebugMessagesBufferTimer)then
		flushDebugMessagesBufferTimer = setTimer(flushDebugMessagesBuffer, 1000, 1);
	end
	debugMessagesBuffer[#debugMessagesBuffer + 1] = {message, level, file, line};
end)
