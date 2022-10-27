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
    void Start() {
        DontDestroyOnLoad(gameObject);
        playerUpdateStruct = new Player();
        instance = this;
    }

    // Update is called once per frame


    public override void NetworkUpdate(Server server, Client client, bool isServer)
    {
        Debug.Log("Network Update Called");
        PlayerMoveMessageSender();        
    }
    public virtual void AddPlayer(ushort id, string username) {
        GameObject Jeff = Instantiate(PlayerPrefab);
        ScenePlayer JeffSP = Jeff.GetComponent<ScenePlayer>();
        scenePlayers.Add(JeffSP);
        JeffSP.ScoreBoardPosition = scenePlayers.IndexOf(JeffSP);
        JeffSP.ID = id;
        JeffSP.name = username;
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
        playerUpdateStruct.Head.Position = GM.CurrentPlayerBody.Head.position;
        playerUpdateStruct.Head.Rotation = GM.CurrentPlayerBody.Head.rotation;
        playerUpdateStruct.LeftHand.Position = GM.CurrentMovementManager.Hands[0].transform.position;
        playerUpdateStruct.LeftHand.Rotation = GM.CurrentMovementManager.Hands[0].transform.rotation;
        playerUpdateStruct.RightHand.Position = GM.CurrentMovementManager.Hands[1].transform.position;
        playerUpdateStruct.RightHand.Rotation =  GM.CurrentMovementManager.Hands[1].transform.rotation;
        Debug.Log("Packed the position/rotation etc");
        msg.Add(playerUpdateStruct);
        Debug.Log("Added Moved Packet");
        Plugin.Instance.Client.Send(msg);

    }
    
}
