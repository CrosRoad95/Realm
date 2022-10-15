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

local _triggerServerEvent = triggerServerEvent;

function triggerServerEvent(eventName, element, ...)
    local id = uuid();
    _triggerServerEvent(eventName, element, id, ...)
    return id
end
