package = "H3MP-server"
version = "dev-1"
source = {
    url = "git+https://github.com/WFIOST/H3MP.git"
}
description = {
    homepage = "https://github.com/WFIOST/H3MP",
    license = "*** please specify a license ***"
}
dependencies = {
    "lua ~> 5.1",
    "luasocket",
    "luasec",
    "penlight"
}
build = {
    type = "builtin",
    install = {
        bin = {
            ["h3mp-server"] = "src/h3mp-server.lua"
        }
    },

    modules = {

    }
}
