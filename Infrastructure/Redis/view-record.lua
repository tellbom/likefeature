-- view-record.lua
-- KEYS[1] = viewed users set key, e.g. "views:users:{newsId}"
-- KEYS[2] = view count key, e.g. "views:count:{newsId}"
-- ARGV[1] = userId
-- Returns: { recorded (1=new, 0=duplicate), viewCount }

local isMember = redis.call('SISMEMBER', KEYS[1], ARGV[1])

if isMember == 0 then
    redis.call('SADD', KEYS[1], ARGV[1])
    local count = redis.call('INCR', KEYS[2])
    return { 1, count }
else
    local count = redis.call('GET', KEYS[2])
    return { 0, count or 0 }
end
