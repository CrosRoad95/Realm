local random = math.random
local template ='xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'

function uuid()
    local result = string.gsub(template, '[xy]', function (c)
        local v = (c == 'x') and random(0, 0xf) or random(8, 0xb)
        return string.format('%x', v)
    end)
    return result
end

function async(calledFunction, ...) 
    local co = coroutine.create(calledFunction)
    coroutine.resume(co, ...)
end 

function triggerServerEventWithId(eventName, ...)
    local id = uuid();
    triggerServerEvent(eventName, resourceRoot, id, ...)
    return id
end

function table.size(tab)
    local length = 0
    for _ in pairs(tab) do length = length + 1 end
    return length
end

local hadTranslations = false
local function hasTranslations()
    if(hadTranslations)then
        return hadTranslations
    end
    local translations = getElementData(localPlayer, "translations") or {}
    if(table.size(translations) ~= 0)then
        hadTranslations = true
        return true;
    end
    return false;
end

function waitForTranslations()
    if(hasTranslations())then
        return;
    end
    local co = coroutine.running() 
    local waitTimer;
    local continue = function()
        if(hasTranslations())then
            coroutine.resume(co)
            killTimer(waitTimer);
            waitTimer = nil
        end
    end
    waitTimer = setTimer(continue, 150, 0) -- TODO: Make it event based
    outputChatBox("waiting fior transl")
    coroutine.yield()
end

function localizationGetString(name, default)
    local translations = getElementData(localPlayer, "translations")
    return translations[name] or default
end

__ = localizationGetString;