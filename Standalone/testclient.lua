local socket = require("socket")

local client = socket.tcp()
client:connect("127.0.0.1", tonumber(arg[1] or 42069))

print(client:receive("*l"))
client:send("Hello from client!")
client:close()
print("done")
