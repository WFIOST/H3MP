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

namespace H3MP.Server
{
    public class Server
    {
        public bool IsServer { get; private set; }

        public Riptide.Server Connection { get; private set; }
        public ulong ServerTick { get; private set; }
        public Dictionary<ushort, Player> Players { get; private set; }

        public Dictionary<ushort, Action<ushort, Message>> ServerMessageHandlers { get; private set; }
        public Dictionary<int, NetworkedObject> Objects { get; private set; }
        private int _objectIDReference;
        public string CurrentSceneID { get; private set; }

        public ulong TickRefreshRate { get; set; }
        
        public Server(ulong tickRefreshRate)
        {
            TickRefreshRate = tickRefreshRate;
            Connection = new Riptide.Server();
            Players = new Dictionary<ushort, Player>();
            Objects = new Dictionary<int, NetworkedObject>();
            CurrentSceneID = "";

            //Regular 
            ServerMessageHandlers = new Dictionary<ushort, Action<ushort, Message>>();
            ServerMessageHandlers[(ushort)MessageIdentifier.Player.ENTER] = OnClientConnection;
            ServerMessageHandlers[(ushort)MessageIdentifier.Player.EXIT] = OnClientDisconnection;
            ServerMessageHandlers[(ushort)MessageIdentifier.Player.UPDATE_TRANSFORM] = OnPlayerMove;
            ServerMessageHandlers[(ushort)MessageIdentifier.Player.UPDATE_INPUT] = OnPlayerInputUpdate;
            ServerMessageHandlers[(ushort)MessageIdentifier.Object.BIRTH] = OnObjectSpawn;
            ServerMessageHandlers[(ushort)MessageIdentifier.Object.DEATH] = OnObjectDespawn;

            //
            Connection.MessageReceived += (_, args) =>
            {
                if (!ServerMessageHandlers.ContainsKey(args.MessageId))
                {
                    RiptideLogger.Log(logType: LogType.Warning,
                        message: String.Format("No handler found for message with id {0}", args.MessageId));
                    return;
                }

                ServerMessageHandlers[args.MessageId](arg1: args.FromConnection.Id, arg2: args.Message);
            };
        }

        public void StartServer(ushort port, ushort maxClients)
        {
            //Just in case!
            Stop();

            //The host connects to the server as a client
            IsServer = true;
            
            Connection.Start(port: port, maxClientCount: maxClients);

            //Reset
            Players = new Dictionary<ushort, Player>();
            Objects = new Dictionary<int, NetworkedObject>();
        }


        public void Update()
        {
            Connection.Update();

            if (ServerTick++ % TickRefreshRate == 0)
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
            var newPlayer = new Player() {
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
                    .AddSerializables(Objects.ToSerialisable()),
                toClient: id);

            Connection.Send(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Networking.PLAYER_LIST)
                    .AddSerializables(Players.Values.ToArray()),
                toClient: id);
            Players[id] = newPlayer;
        }

        public void OnClientDisconnection(ushort id, Message pkt)
        {
            var reason = (DisconnectReason)pkt.GetByte();

            RiptideLogger.Log(logType: LogType.Info, message: String.Format("Client {0} disconnected with reason {1}", id, reason.ToString()));
            Players.Remove(id);

            Connection.SendToAll(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Player.EXIT)
                    .Add(id)
                    .Add((byte)reason),
                exceptToClientId: id);
        }

        public void OnPlayerMove(ushort id, Message pkt)
        {
            var player = pkt.GetSerializable<Player>();
            if (player.ID != id)
                throw new Exception(String.Format("Player id ({0}) does not match id from sent message ({1})!", player.ID, id));

            Players[id] = player;

            var msg = Message.Create(sendMode: MessageSendMode.Unreliable, id: MessageIdentifier.Player.UPDATE_TRANSFORM)
                .Add(id)
                .Add(player);

            Connection.SendToAll(message: msg, exceptToClientId: player.ID);
        }

        public void OnPlayerInputUpdate(ushort id, Message pkt)
        {
            byte rawid = pkt.GetByte();
            bool isRightHand = (rawid & 0x80) == 0;
            var inputID = (InputIdentifier)(isRightHand ? (rawid ^ 0x80) : rawid);

            var val = (isRightHand ? Players[id].RightHand : Players[id].LeftHand).Input.UpdateInput(id: inputID, msg: pkt);
            var msg = Message.Create(sendMode: MessageSendMode.Unreliable, id: MessageIdentifier.Player.UPDATE_INPUT)
                .Add(id)
                .Add((byte)inputID);

            val.Switch(
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
}
