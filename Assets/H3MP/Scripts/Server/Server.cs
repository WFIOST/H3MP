using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Riptide;
using Riptide.Utils;
using BepInEx;
using UnityEngine;
using OneOf;
using LogType = Riptide.Utils.LogType;

namespace H3MP.Server;

public delegate void OnServerTick(Riptide.Server server);

public class Server
{
    public bool IsServer { get; private set; }

    public Riptide.Server Connection { get; }
    public ConfigEntry<ushort> PlayerCount { get; }
    public ConfigEntry<ushort> Port { get; }
    public ConfigEntry<ushort> TickRefreshRate { get; }
    
    public ulong ServerTick { get; private set; }
    public Dictionary<ushort, Player> Players { get; private set; }
    
    public Dictionary<ushort, Action<ushort, Message>> ServerMessageHandlers { get; }
    public Dictionary<int, NetworkedObject> Objects { get; private set; }
    private int _objectIDReference;
    public string CurrentSceneID { get; private set; }

    public Server()
    {
        Port            = Plugin.Instance!.Config.Bind(section: "Server",
                                      key: "Port",
                                      defaultValue: (ushort)42069,
                                      description: "Port to run the server on, must be port forwarded");
        
        PlayerCount     = Plugin.Instance!.Config.Bind(section: "Server",
                                      key: "Max player count",
                                      defaultValue: (ushort)0x10,
                                      description: "Maximum amount of players allowed on the server");
        
        TickRefreshRate = Plugin.Instance!.Config.Bind(section: "Server",
                                      key: "Tick refresh rate",
                                      defaultValue: (ushort)0xFF,
                                      description: "How many ticks until the server syncs and refreshes");
        
        //TODO: We really need to split the client and server into seperate classes (maybe make the client only in the MeatKit project?)
        Connection = new Riptide.Server();
        Players = new Dictionary<ushort, Player>();
        Objects = new Dictionary<int, NetworkedObject>();
        CurrentSceneID = "";
        
        //Regular 
        ServerMessageHandlers = new Dictionary<ushort, Action<ushort, Message>>
        {
            [(ushort)MessageIdentifier.Player.ENTER]            = OnClientConnection, 
            [(ushort)MessageIdentifier.Player.EXIT]             = OnClientDisconnection, 
            [(ushort)MessageIdentifier.Player.UPDATE_TRANSFORM] = OnPlayerMove,
            [(ushort)MessageIdentifier.Player.UPDATE_INPUT]     = OnPlayerInputUpdate,
            
            [(ushort)MessageIdentifier.Object.BIRTH]            = OnObjectSpawn,
            [(ushort)MessageIdentifier.Object.DEATH]            = OnObjectDespawn,
        };

        //
        Connection.MessageReceived += (_, args) =>
        {
            if (!ServerMessageHandlers.ContainsKey(args.MessageId))
            {
                RiptideLogger.Log(logType: LogType.Warning,
                                  message: $"No handler found for message with id {args.MessageId}");
                return;
            }

            ServerMessageHandlers[args.MessageId](arg1: args.FromConnection.Id, arg2: args.Message);
        };
    }

    public void StartServer(ushort port = 0, ushort maxClients = 0)
    {
        //Just in case!
        Stop();

        //The host connects to the server as a client
        IsServer = true;
        port = port == 0 ? Port.Value : port;
        Connection.Start(port: port == 0 ? Port.Value : port, maxClientCount: maxClients == 0 ? PlayerCount.Value : maxClients);

        //Reset
        Players = new Dictionary<ushort, Player>();
        Objects = new Dictionary<int, NetworkedObject>();
    }
    
   
    public void Update()
    {
        Connection.Update();

        if (ServerTick++ % TickRefreshRate.Value == 0)
            Connection.SendToAll(message: Message.Create(sendMode: MessageSendMode.Unreliable,
                                                     id: MessageIdentifier.Networking.SYNC)
                                             .Add(ServerTick));
    }

    public void Stop()
    {
        Connection.Stop();
    }

    #region Players

    public void OnClientConnection(ushort id, Message pkt)
    {
        var newPlayer = new Player()
        {
            ID = id,
            Username = pkt.GetString()
        };
        
        Connection.SendToAll(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Player.ENTER)
                                         .Add(id)
                                         .Add(newPlayer),
                         exceptToClientId: id);
        
        Connection.Send(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Scene.LOAD)
                                    .Add(CurrentSceneID),
                    toClient: id);
        
        Connection.Send(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Networking.OBJECT_LIST)
                                    .Add(Objects.ToSerialisable()),
                    toClient: id);
        
        Connection.Send(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Networking.PLAYER_LIST)
                                    .Add(Players.Values.ToArray()),
                    toClient: id);
        Players[id] = newPlayer;
    }
    
    public void OnClientDisconnection(ushort id, Message pkt)
    {
        var reason = (DisconnectReason)pkt.GetByte();
        
        RiptideLogger.Log(logType: LogType.Info, message: $"Client {id} disconnected with reason {reason.ToString()}");
        Players.Remove(id);

        Connection.SendToAll(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Player.EXIT)
                                         .Add(id)
                                         .Add((byte) reason), 
                         exceptToClientId: id);
    }

    public void OnPlayerMove(ushort id, Message pkt)
    {
        var player = pkt.GetSerializable<Player>();
        if (player.ID != id)
            throw new Exception($"Player id ({player.ID}) does not match id from sent message ({id})!");

        Players[id] = player;

        var msg = Message.Create(sendMode: MessageSendMode.Unreliable, id: MessageIdentifier.Player.UPDATE_TRANSFORM)
                                .Add(id)
                                .Add(player);
        
        Connection.SendToAll(message: msg, exceptToClientId: player.ID);
    }

    public void OnPlayerInputUpdate(ushort id, Message pkt)
    {
        byte rawid = pkt.GetByte();
        bool isRightHand = (rawid & 0b10000000) == 0;
        var inputID = (InputIdentifier)(isRightHand ? (rawid ^ 0b10000000) : rawid);
        
        var val = (isRightHand ? Players[id].RightHand : Players[id].LeftHand).Input.UpdateInput(id: inputID, msg: pkt);
        var msg = Message.Create(sendMode: MessageSendMode.Unreliable, id: MessageIdentifier.Player.UPDATE_INPUT)
                                .Add(id)
                                .Add((byte) inputID);

        val.Switch (
            b => msg.Add(b),
            f => msg.Add(f),
            v => msg.AddVector2(v),
            v => msg.AddVector3(v),
            q => msg.AddQuaternion(q)
        );

        Connection.SendToAll(message: msg, exceptToClientId: id);
    }
#endregion

#region Objects
    public void OnObjectSpawn(ushort id, Message msg)
    {
        int tmpID = msg.GetInt();
        var obj = msg.GetSerializable<NetworkedObject>();
        obj.ID = _objectIDReference++;
        Objects[obj.ID] = obj;

        Connection.SendToAll(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Object.BIRTH)
                                         .Add(obj),
                         exceptToClientId: id);
        Connection.Send(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Object.ID_SET)
                                    .Add(tmpID)
                                    .Add(obj.ID),
                    toClient: id);
    }
    
    public void OnObjectDespawn(ushort id, Message msg)
    {
        var objid = msg.GetInt();
        var newmsg = Message.Create(MessageSendMode.Reliable, MessageIdentifier.Object.DEATH)
                                   .Add(objid);
        Connection.SendToAll(message: newmsg, exceptToClientId: id);
        Objects.Remove(objid);
    }
#endregion

#region Scene

    public void OnSceneSwitch(ushort id, Message msg)
    {
        CurrentSceneID = msg.GetString();
        
        Connection.SendToAll(message: msg, exceptToClientId: id);
    }

#endregion
}