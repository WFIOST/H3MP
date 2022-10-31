using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using H3MP.Core;
using System.Linq;
using FistVR;
using H3MP.Scripts;

public class NetworkManager : MonoBehaviour
{
    public static Action PlayerConnectedEvent;

    public List<ScenePlayer> scenePlayers = new List<ScenePlayer>();
    public GameObject PlayerPrefab;
    public List<string> PlayerList;
    public static NetworkManager instance;

    private Plugin _networking
    {
        get
        {
            if (Plugin.Instance == null) throw new Exception("Plugin is null? How can this be?");
            return Plugin.Instance;
        }
    }

    private Player playerUpdateStruct;
    private bool hasConnected;
    private bool hasUpdatedID;

    private Dictionary<int, NetworkedBehaviour> _objects;

    // Use this for initialization
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        playerUpdateStruct = new Player();
        instance = this;
        _objects = new Dictionary<int, NetworkedBehaviour>();
        _tmpIDMap = new Dictionary<int, NetworkedBehaviour>();

        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Networking.PLAYER_LIST] = HandlePlayerListMessages;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Player.UPDATE_TRANSFORM] =
            HandlePlayerMovementMessage;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Player.ENTER] =
            HandleConnectionInformationPacketMessage;

        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Object.BIRTH] = OnObjectBirth;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Object.ID_SET] = TempIDToRealID;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Object.DEATH] = OnDeathProclamation;
    }


    void Update()
    {
        playerUpdateStruct.Head.Position = GM.CurrentPlayerBody.Head.position;
        playerUpdateStruct.Head.Rotation = GM.CurrentPlayerBody.Head.rotation;
        playerUpdateStruct.LeftHand.Position = GM.CurrentMovementManager.Hands[0].transform.position;
        playerUpdateStruct.LeftHand.Rotation = GM.CurrentMovementManager.Hands[0].transform.rotation;
        playerUpdateStruct.RightHand.Position = GM.CurrentMovementManager.Hands[1].transform.position;
        playerUpdateStruct.RightHand.Rotation = GM.CurrentMovementManager.Hands[1].transform.rotation;
        if (!hasUpdatedID && _networking.Client.IsConnected)
        {
            playerUpdateStruct.ID = _networking.Client.Id;
            playerUpdateStruct.Username = _networking.Username.Value;
            hasUpdatedID = true;
        }
    }

    public void FixedUpdate()
    {
        if (_networking.Client.IsConnected)
        {
            PlayerMoveMessageSender();
        }
    }

#region Players

    public virtual void AddPlayer(ushort id, string username)
    {
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

    //Handles incoming player mocement packets
    public void HandlePlayerMovementMessage(Message message)
    {

        Player NewMovePacket = new Player();

        NewMovePacket.Deserialize(message);
        // Debug.Log("Move Packet ID " + NewMovePacket.ID.ToString());
        for (int i = 0; i <= scenePlayers.Count - 1; i++)
        {
            if (scenePlayers[i].ID == NewMovePacket.ID)
            {
                scenePlayers[i].UpdatePlayer(NewMovePacket);
            }
        }
    }

    private void PlayerMoveMessageSender()
    {
        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort) MessageIdentifier.Player.UPDATE_TRANSFORM);

        msg.Add(playerUpdateStruct);
        _networking.Client.Send(msg);

    }

#endregion


#region Objects

    private int _tempIDReference;
    private Dictionary<int, NetworkedBehaviour> _tmpIDMap;

    public void RegisterObject(NetworkedBehaviour obj)
    {
        _tmpIDMap[_tempIDReference] = obj;
        _networking.Client.Send(Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Object.BIRTH)
                                       .Add(_tempIDReference)
                                       .Add(obj.Data));
        _tempIDReference++;
    }

    public void MurderObject(int id)
    {
        _networking.Client.Send(Message.Create(sendMode: MessageSendMode.Reliable, id: MessageIdentifier.Object.DEATH)
                                       .Add(id));
        KillObj(id);
    }

    private void TempIDToRealID(Message msg)
    {
        int tmp = msg.GetInt(), real = msg.GetInt();
        _tmpIDMap[tmp].SetID(real);
        _objects[real] = _tmpIDMap[tmp];
        _tmpIDMap.Remove(tmp);
    }

    private void OnDeathProclamation(Message msg)
    {
        KillObj(id: msg.GetInt());
    }

    private void KillObj(int id)
    {
        _objects[id].Die();
        _objects.Remove(id);
    }


    private void OnObjectBirth(Message msg)
    {
        
    }
#endregion
}
