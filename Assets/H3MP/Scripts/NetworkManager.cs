using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using H3MP.Core;
using System.Linq;
using FistVR;
public class NetworkManager : MonoBehaviour
{
    public static Action PlayerConnectedEvent;
    
    public List<ScenePlayer> scenePlayers = new List<ScenePlayer>();
    public GameObject PlayerPrefab;
    public List<string> PlayerList;
    public static NetworkManager instance;
    private Player playerUpdateStruct;
    private bool hasConnected = false;
    private bool hasUpdatedID = false;
    // Use this for initialization
    void Awake() {
        
        DontDestroyOnLoad(gameObject);
        playerUpdateStruct = new Player();
        instance = this;
       
        Plugin.Instance.ClientMessageHandlers[(ushort)MessageIdentifier.Networking.PLAYER_LIST] = HandlePlayerListMessages; 
        Plugin.Instance.ClientMessageHandlers[(ushort)MessageIdentifier.Player.UPDATE_TRANSFORM] = HandlePlayerMovementMessage;
        Plugin.Instance.ClientMessageHandlers[(ushort)MessageIdentifier.Player.ENTER] = HandleConnectionInformationPacketMessage;
        Plugin.Instance.ClientMessageHandlers[(ushort)MessageIdentifier.Networking.SYNC] = HandleSyncMessage;
    }


    void Update()
    {      
       playerUpdateStruct.Head.Position = GM.CurrentPlayerBody.Head.position;
       playerUpdateStruct.Head.Rotation = GM.CurrentPlayerBody.Head.rotation;
       playerUpdateStruct.LeftHand.Position = GM.CurrentMovementManager.Hands[0].transform.position;
       playerUpdateStruct.LeftHand.Rotation = GM.CurrentMovementManager.Hands[0].transform.rotation;
       playerUpdateStruct.RightHand.Position = GM.CurrentMovementManager.Hands[1].transform.position;
       playerUpdateStruct.RightHand.Rotation = GM.CurrentMovementManager.Hands[1].transform.rotation;
        if (!hasUpdatedID && Plugin.Instance.Client.IsConnected)
        {
            playerUpdateStruct.ID = Plugin.Instance.Client.Id;
            playerUpdateStruct.Username = Plugin.Instance.Username.Value;
            hasUpdatedID = true;
        }
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
        
        if (PlayerConnectedEvent != null)
            PlayerConnectedEvent.Invoke();
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
   
    public void HandlePlayerListMessages(Message message)
    {
        Player[] Players = message.GetSerializables<Player>();
        foreach (Player player in Players)
        {
            AddPlayer(player.ID, player.Username);
        }
    }
    //Handles Handles the message at connection where the server sends the client a list of currently connected players
    public void HandleConnectionInformationPacketMessage(Message message)
    {
        AddPlayer(message.GetUShort(), message.GetString());
        hasConnected = true;
    }
    public void HandleSyncMessage(Message message)
    { 
    
    }
        //Handles incoming player mocement packets
    public void HandlePlayerMovementMessage(Message message)
    {

        Player NewMovePacket = new Player();
       
       NewMovePacket.Deserialize(message);
        Debug.Log("Move Packet ID " + NewMovePacket.ID.ToString());
        for (int i = 0; i <= scenePlayers.Count-1; i++)
        {
            if (scenePlayers[i].ID == NewMovePacket.ID)
            {
                scenePlayers[i].UpdatePlayer(NewMovePacket);
            }
        }
    }
    private void PlayerMoveMessageSender()
    {
        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort)MessageIdentifier.Player.UPDATE_TRANSFORM);
       
        //Debug.Log("Packed the position/rotation etc");
        msg.Add(playerUpdateStruct);
        //Debug.Log("Added Moved Packet");
        Plugin.Instance.Client.Send(msg);
        //Debug.Log("SendingPacket");

    }
    
}
