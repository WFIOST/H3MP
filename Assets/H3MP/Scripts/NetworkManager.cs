using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using H3MP.Core;
using H3MP.Common;
using FistVR;
public class NetworkManager : NetworkedBehaviour {

    public List<ScenePlayer> scenePlayers = new List<ScenePlayer>();
    public GameObject PlayerPrefab;
    public List<string> PlayerList;
    public static NetworkManager instance;
    private PlayerStruct playerUpdateStruct;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame


    public override void NetworkUpdate(Server server, Client client, bool isServer)
    {
        PlayerMoveMessageSender();
        
    }
    public virtual void AddPlayer(ushort id, string username) {
        GameObject Jeff = Instantiate(PlayerPrefab);
        ScenePlayer JeffSP = Jeff.GetComponent<ScenePlayer>();
        scenePlayers.Add(JeffSP);
        JeffSP.ScoreBoardPosition = scenePlayers.IndexOf(JeffSP);
        JeffSP.ID = id;
        JeffSP.name = username;

    }
    public virtual void ScoreBoardUpdate()
    { 
        //this will be used to update the networkingPanel's scoreboard UI
    }
    public virtual void ScoreBoardUpdate(ushort ID)
    {

        for (int i = 0; i < scenePlayers.Count; i++)
        {
            if (scenePlayers[i].ID == ID)
            {
                PlayerList.RemoveAt(i);
                break;
            }
        }
        foreach (ScenePlayer PS in scenePlayers)
        {
            PS.ScoreBoardPosition = scenePlayers.IndexOf(PS);
        }

    }



    [MessageHandler((ushort)MessageIdentifier.Player.ENTER)]
    private static void HandleConnectionInformationPacketMessage(Message message)
    {
        instance.AddPlayer(message.GetUShort(), message.GetString());
    }
    [MessageHandler((ushort)MessageIdentifier.Player.MOVED)]
    private static void HandlePlayerMovementMessage(Message message)
    {

        PlayerStruct NewMovePacket = new PlayerStruct();
       NewMovePacket.Deserialize(message);
        for (int i = 0; i < instance.scenePlayers.Count; i++)
        {
            if (instance.scenePlayers[i].ID == NewMovePacket.ID)
            {
                instance.scenePlayers[i].UpdatePlayer(NewMovePacket);
            }
        }
    }
    private void PlayerMoveMessageSender()
    {
        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort)MessageIdentifier.Player.MOVED);
        playerUpdateStruct = new PlayerStruct(ushort.MaxValue, GM.CurrentPlayerBody.Head.position, GM.CurrentPlayerBody.Head.rotation, GM.CurrentMovementManager.Hands[0].transform.position, GM.CurrentMovementManager.Hands[0].transform.rotation, GM.CurrentMovementManager.Hands[1].transform.position, GM.CurrentMovementManager.Hands[1].transform.rotation) ;
        //playerUpdateStruct.Serialize(msg);
        msg.Add(playerUpdateStruct);
        Plugin.Instance.Client.Send(msg);

    }
    /*public struct PlayerStruct : IEquatable<PlayerStruct>, IMessageSerializable
    {
        public Player Player;
        public ushort ID;
       
        public PlayerStruct(ushort mID, Vector3 HP, Quaternion HR, Vector3 LHP, Quaternion LHR, Vector3 RHP, Quaternion RHR)
        {
            ID = mID;
            Player p = new Player
            {
                headPos = HP,
                headRot = HR,
                leftHandPos = LHP,
                leftHandRot = LHR,
                rightHandPos = RHP,
                rightHandRot = RHR
            };
            Player = p;

        }
        public bool Equals(PlayerStruct other)
        {
            if (ID == other.ID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void Serialize(Message message)
        {
            Serialization.Add(message, Player.headPos);
            Serialization.Add(message, Player.headRot);
            Serialization.Add(message, Player.leftHandPos);
            Serialization.Add(message, Player.leftHandRot);
            Serialization.Add(message, Player.rightHandPos);
            Serialization.Add(message, Player.rightHandRot);
        }
        public void Deserialize(Message message)
        {
            ID = message.GetUShort();
            Player.headPos = message.GetVector3();
            Player.headRot = message.GetQuaternion();
            Player.leftHandPos = message.GetVector3();
            Player.leftHandRot = message.GetQuaternion();
            Player.rightHandPos = message.GetVector3();
            Player.rightHandRot = message.GetQuaternion();
        }
    }*/
    

}
