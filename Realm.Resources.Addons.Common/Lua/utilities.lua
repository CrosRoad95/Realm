local random = math.random
local template ='xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'

local _triggerServerEvent = triggerServerEvent;

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


function triggerServerEvent(eventName, ...)
    local id = uuid();
    _triggerServerEvent(eventName, resourceRoot, id, ...)
    return id
end
