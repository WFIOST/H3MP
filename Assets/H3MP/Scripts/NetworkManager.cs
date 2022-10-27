using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using H3MP.Core;

using FistVR;
public class NetworkManager : NetworkedBehaviour {
    

    public List<ScenePlayer> scenePlayers = new List<ScenePlayer>();
    public GameObject PlayerPrefab;
    public List<string> PlayerList;
    public static NetworkManager instance;
    private Player playerUpdateStruct;
    // Use this for initialization
    void Awake() {
        
        DontDestroyOnLoad(gameObject);
        playerUpdateStruct = new Player();
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
       instance.playerUpdateStruct.Head.Position = GM.CurrentPlayerBody.Head.position;
       instance.playerUpdateStruct.Head.Rotation = GM.CurrentPlayerBody.Head.rotation;
       instance.playerUpdateStruct.LeftHand.Position = GM.CurrentMovementManager.Hands[0].transform.position;
       instance.playerUpdateStruct.LeftHand.Rotation = GM.CurrentMovementManager.Hands[0].transform.rotation;
       instance.playerUpdateStruct.RightHand.Position = GM.CurrentMovementManager.Hands[1].transform.position;
       instance.playerUpdateStruct.RightHand.Rotation = GM.CurrentMovementManager.Hands[1].transform.rotation;
    }

    public override void NetworkUpdate(Server server, Client client, bool isServer)
    {
        if (Plugin.Instance.Client.IsConnected)
        {
           // Debug.Log("Network Update Called");
            PlayerMoveMessageSender();
        }
    }
    public virtual void AddPlayer(ushort id, string username) {
        GameObject Jeff = Instantiate(PlayerPrefab);
        ScenePlayer JeffSP = Jeff.GetComponent<ScenePlayer>();
        scenePlayers.Add(JeffSP);
        JeffSP.ScoreBoardPosition = scenePlayers.IndexOf(JeffSP);
        JeffSP.ID = id;
        JeffSP.Name = username;
        JeffSP.namePlate.text = username;

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

    [MessageHandler((ushort)MessageIdentifier.Networking.PLAYER_LIST)]
    public static void HandlePlayerListMessages(Message message)
    {
        Player[] Players = message.GetSerializables<Player>();
        foreach (Player player in Players)
        {
            instance.AddPlayer(player.ID, player.Username);
        }
    }
    [MessageHandler((ushort)MessageIdentifier.Player.ENTER)]
    public static void HandleConnectionInformationPacketMessage(Message message)
    {
        instance.AddPlayer(message.GetUShort(), message.GetString());
        
    }
    [MessageHandler((ushort)MessageIdentifier.Player.MOVED)]
    public static void HandlePlayerMovementMessage(Message message)
    {

        Player NewMovePacket = new Player();
       
       NewMovePacket.Deserialize(message);
        Debug.Log("Move Packet ID " + NewMovePacket.ID.ToString());
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
       
        //Debug.Log("Packed the position/rotation etc");
        msg.Add(instance.playerUpdateStruct);
        //Debug.Log("Added Moved Packet");
        Plugin.Instance.Client.Send(msg);
        Debug.Log("SendingPacket");

    }
    
}
