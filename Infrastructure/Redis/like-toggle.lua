-- like-toggle.lua
-- KEYS[1] = liked_users set key,  e.g. "likes:users:{newsId}"
-- KEYS[2] = like_count key,       e.g. "likes:count:{newsId}"
-- ARGV[1] = userId
--
-- Returns: array { liked (1=liked / 0=unliked), likeCount }

local isMember = redis.call('SISMEMBER', KEYS[1], ARGV[1])

if isMember == 0 then
    redis.call('SADD', KEYS[1], ARGV[1])
    local count = redis.call('INCR', KEYS[2])
    return { 1, count }
else
    redis.call('SREM', KEYS[1], ARGV[1])
    local count = redis.call('DECR', KEYS[2])
    if count < 0 then
        redis.call('SET', KEYS[2], 0)
        count = 0
    end
    return { 0, count }
end
