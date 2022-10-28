using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using H3MP.Core;
using System.Linq;
using FistVR;
public class NetworkManager : MonoBehaviour {
    

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

   [MessageHandler((ushort)MessageIdentifier.ToServer.Player.ENTER)]
    public static void OnClientConnection(Message pkt)
    {
        ushort id = pkt.GetUShort();
        string user = pkt.GetString();

        var player = new Player(userID: id, username: user);
        Message msg = Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.ToClient.Player.ENTER)
                             .Add(player);

        foreach (Player p in Plugin.Instance.Players.Where(p => p.ID != id))
            Plugin.Instance.Server.Send(message: msg, toClient: p.ID);

        Plugin.Instance.Players.Add(player);
        //Plugin.Instance.Server.Send(message: Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.ToClient.Networking.PLAYER_LIST).Add(Plugin.Instance.Players.ToArray()),toClient: id);
    }

    [MessageHandler((ushort)MessageIdentifier.ToServer.Player.UPDATE_TRANSFORM)]
    public static void OnPlayerMove(Message pkt)
    {
        var player = pkt.GetSerializable<Player>();

        Plugin.Instance.Players[Plugin.Instance.Players.IndexOf(player)] = player;

        Message msg = Message.Create(sendMode: MessageSendMode.Unreliable, id: MessageIdentifier.ToClient.Player.UPDATE_TRANSFORM)
                             .Add(player);

        foreach (Player p in Plugin.Instance.Players.Where(p => p.ID != player.ID))
            Plugin.Instance.Server.Send(message: msg, toClient: p.ID);
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

    public void FixedUpdate()
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

    [MessageHandler((ushort)MessageIdentifier.ToClient.Networking.PLAYER_LIST)]
    public static void HandlePlayerListMessages(Message message)
    {
        Player[] Players = message.GetSerializables<Player>();
        foreach (Player player in Players)
        {
            instance.AddPlayer(player.ID, player.Username);
        }
    }
    [MessageHandler((ushort)MessageIdentifier.ToClient.Player.ENTER)]
    public static void HandleConnectionInformationPacketMessage(Message message)
    {
        instance.AddPlayer(message.GetUShort(), message.GetString());
        
    }
    [MessageHandler((ushort)MessageIdentifier.ToClient.Player.UPDATE_TRANSFORM)]
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
        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort)MessageIdentifier.ToServer.Player.UPDATE_TRANSFORM);
       
        //Debug.Log("Packed the position/rotation etc");
        msg.Add(instance.playerUpdateStruct);
        //Debug.Log("Added Moved Packet");
        Plugin.Instance.Client.Send(msg);
        //Debug.Log("SendingPacket");

    }
    
}
