
using System;
using System.Collections.Generic;
using BepInEx;
using Riptide;
using Riptide.Utils;

namespace H3MP.Server;


[BepInPlugin(GUID: "org.wfiost.h3mp.server", Name: "H3MP - Server", Version: "0.1.0")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin? Instance { get; private set; }

    //TODO: Split the constructor into the separate Client.cs and Server.cs
    public Plugin()
    {
        RiptideLogger.Initialize(debugMethod:       Logger.LogDebug,
                                 infoMethod:        Logger.LogInfo,
                                 warningMethod:     Logger.LogWarning,
                                 errorMethod:       Logger.LogError,
                                 includeTimestamps: true);
        
        //set up default user config data

        // Username                = Config.Bind(section:      "Client",
        //                                       key:          "Username",
        //                                       defaultValue: "Frit's bitch",
        //                                       description:  "If not defined, will use Steam's UserID");
        //
        // TickDivergenceTolerance = Config.Bind(section: "Client",
        //                                       key:              "Tick divergence tolerance",
        //                                       defaultValue: (ulong)0xFF,
        //                                       description: "How many ticks until the client attempts to correct itself");

        // Server = new Server();
        Instance = this;
    }
    

    private void FixedUpdate()
    {
        // Server.Update();
    }

    private void OnDestroy()
    {
        // Server.Stop();
    }
}
