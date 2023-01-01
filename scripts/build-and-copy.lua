local FILES_TO_COPY = {
    ["H3MP/Core/bin/Debug/net35/"] = {
        "H3MP.Core.dll",
        "RiptideNetworking.dll",
    }
}


local function exec(msg, cmd, args)
    io.write("\x1b[33m" .. msg .. "\x1b[0m\t")
    local ret

    if type(cmd) == "function" then
        ret = cmd(args)
    else
        local proc, err = io.popen(cmd .. " " .. table.concat(args, ' '), "r")
        if proc == nil then error("\x1b[31mCould not execute " .. msg .. "! Reason: " .. err .. "\x1b[0m") end

        ret = proc:read("a")
        proc:close()
    end

    io.write "\x1b[32m[Done]\x1b[0m\n"
    return ret
end

exec("Updating project...", "git", { "pull", "--recurse-submodules" })
exec("Updating submodules...", "git", { "submodule", "update", "--remote", "--recursive" })

exec("Building project...", "dotnet", { "build" })

---@param src string
---@param dst string
local function copy(src, dst)
    local r, w = (io.open(src, "rb")), (io.open(dst, "w+b"))
    if r == nil or w == nil then error("Could not copy " .. src .. " to " .. dst) end

    w:write(r:read("a"))

    r:close()
    w:close()
end

exec("Copying files...", function ()
    for dir, files in pairs(FILES_TO_COPY) do
        for _, file in ipairs(files) do copy(dir .. file, "MeatKit/Assets/Managed/" .. file) end
    end
end)
