local socket = require("socket")
-- local ssl = require("ssl")
local pretty = require("pl.pretty")

local lprint, lerror = print, error

-- ---Regular print can't print tables, and pprint has quotations around strings, so this is the best solution
-- ---@param ... any
function print(...)
    local a = {...}
    if #a < 1 then return
    elseif #a == 1 then
        if type(a[1]) ~= "table" then lprint(tostring(a[1])) else print(pretty.write(a[1])) end
    else print(pretty.write(a)) end
end

function error(msg, i)
    if type(msg) == "table" then msg = pretty.write(msg)
    elseif type(msg) ~= "string" then msg = tostring(msg) end
    return lerror("\n\x1b[31m"..msg.."\x1b[0m", i)
end

local server = socket.tcp()
server:setoption("reuseaddr", true)
local ok = server:bind("127.0.0.1", tonumber(arg[1] or 42069))
if not ok then error("Could not bind server to localhost!") end
server:listen()
print("Listening")

local client = server:accept()
print("Accepted")

while true do
    print(client:receive("*l"))
    client:send((io.read("*l"))..'\n')
end
