using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using H3MP.Core;
using System.Linq;
using FistVR;
using H3MP.Scripts;
using HarmonyLib;
public class NetworkManager : MonoBehaviour
{
    public static Action PlayerConnectedEvent;

    public List<ScenePlayer> scenePlayers = new List<ScenePlayer>();
    public GameObject PlayerPrefab;
    public List<string> PlayerList;
    public static NetworkManager instance;

    private FVRViveHand _playerLeftHand, _playerRightHand;
    
    private Plugin _networking
    {
        get
        {
            if (Plugin.Instance == null) throw new Exception("Plugin is null? How can this be?");
            return Plugin.Instance;
        }
    }

    public Player playerUpdateStruct;
    
    private bool hasUpdatedID;

    private Dictionary<int, NetworkedBehaviour> _objects;

    // Use this for initialization
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        playerUpdateStruct = new Player();
        playerUpdateStruct.LeftHand = new Hand(GM.CurrentMovementManager.Hands[0]);
        playerUpdateStruct.RightHand = new Hand(GM.CurrentMovementManager.Hands[1]);
        instance = this;
        _objects = new Dictionary<int, NetworkedBehaviour>();
        _tmpIDMap = new Dictionary<int, NetworkedBehaviour>();

        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Networking.PLAYER_LIST] = HandlePlayerListMessages;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Player.UPDATE_TRANSFORM] = HandlePlayerMovementMessage;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Player.ENTER] = HandlePlayerEnterMessage;

        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Object.BIRTH] = OnObjectBirth;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Object.ID_SET] = TempIDToRealID;
        _networking.ClientMessageHandlers[(ushort) MessageIdentifier.Object.DEATH] = OnDeathProclamation;

        _networking.ClientMessageHandlers[(ushort)MessageIdentifier.Player.UPDATE_INPUT] = HandleInput;

        _playerLeftHand = GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>();
        _playerRightHand = GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>();

        var harmony = new Harmony("com.wfiost.patches.h3mp");
        harmony.PatchAll(typeof(FVRViveHand_Hooks));
    }


    void Update()
    {
        playerUpdateStruct.Head.Position = GM.CurrentPlayerBody.Head.position;
        //Debug.Log("1");
        playerUpdateStruct.Head.Rotation = GM.CurrentPlayerBody.Head.rotation;
       // Debug.Log("2");
        playerUpdateStruct.LeftHand.Transform.Position = GM.CurrentMovementManager.Hands[0].transform.position;
       // Debug.Log("3");
        playerUpdateStruct.LeftHand.Transform.Rotation = GM.CurrentMovementManager.Hands[0].transform.rotation;
       // Debug.Log("4");
        playerUpdateStruct.RightHand.Transform.Position = GM.CurrentMovementManager.Hands[1].transform.position;
      //  Debug.Log("5");
        playerUpdateStruct.RightHand.Transform.Rotation = GM.CurrentMovementManager.Hands[1].transform.rotation;
       // Debug.Log("6");
        playerUpdateStruct.LeftHand.Input.Input = _playerLeftHand.Input;
       // Debug.Log("7");
        playerUpdateStruct.RightHand.Input.Input = GM.CurrentMovementManager.Hands[1].Input;
       // Debug.Log("8");
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

    public virtual void AddPlayer(ushort id, Player player)
    {
        GameObject Jeff = Instantiate(PlayerPrefab);
        ScenePlayer JeffSP = Jeff.GetComponent<ScenePlayer>();
        scenePlayers.Add(JeffSP);
        JeffSP.ScoreBoardPosition = scenePlayers.IndexOf(JeffSP);
        JeffSP.ID = id;
        JeffSP.Name = player.Username;
        JeffSP.namePlate.text = player.Username;

        if (PlayerConnectedEvent != null)
            PlayerConnectedEvent.Invoke();
    }
   /* public void sendInput()
    {
        playerUpdateStruct.LeftHand.Input.Input = GM.CurrentMovementManager.Hands[0].Input;
        playerUpdateStruct.LeftHand.Input.Input = GM.CurrentMovementManager.Hands[1].Input;
    }*/
    public void HandlePlayerListMessages(Message message)
    {
        Player[] Players = message.GetSerializables<Player>();
        foreach (Player player in Players)
        {
            AddPlayer(player.ID, player);
        }
    }

    //Handles Handles the message at connection where the server sends the client a list of currently connected players
    public void HandlePlayerEnterMessage(Message message)
    {
        var id = message.GetUShort();
        var player = message.GetSerializable<Player>();
        AddPlayer(id, player);       
    }

    //Handles incoming player mocement packets
    public void HandlePlayerMovementMessage(Message message)
    {
        var id = message.GetUShort();
        var player = message.GetSerializable<Player>();
        
        foreach (var instance in scenePlayers)
        {
            if (instance.ID == id)
                instance.UpdatePlayer(player);
        }
    }

    private void PlayerMoveMessageSender()
    {
        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort)MessageIdentifier.Player.UPDATE_TRANSFORM);

        msg.Add(playerUpdateStruct);
        _networking.Client.Send(msg);

       // _networking.Client.Send(Message.Create(MessageSendMode.Unreliable, MessageIdentifier.Player.UPDATE_INPUT).Add(new SerialisableInput(_playerLeftHand)));
       // _networking.Client.Send(Message.Create(MessageSendMode.Unreliable, MessageIdentifier.Player.UPDATE_INPUT).Add(new SerialisableInput(_playerRightHand)));
    }
    
    private void HandleInput(Message msg)
    {
        //var id = msg.GetUShort();
        //var input = msg.GetSerializable<SerialisableInput>();

        //scenePlayers.First(p => p.ID == id).InputUpdate(input);
    }

    // Previous hand inputs.
    private HandInput _LPrev = new HandInput();
    private HandInput _RPrev = new HandInput();

    private bool _firstRun = true;
    
    private void CheckInputStatus()
    {
        if (_firstRun)
        {
            _LPrev = _playerLeftHand.Input;
            _RPrev = _playerRightHand.Input;
            _firstRun = false;
        }
        var _LCurrent = GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>().Input;
        var _RCurrent = GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>().Input;
        // Check what properties have changed.
        var a = GetChangedProperties(_LPrev, _LCurrent);
        var b = GetChangedProperties(_RPrev, _RCurrent);

        // Update previous values.
        _LPrev = _LCurrent;
        _RPrev = _RCurrent;

        // Hate this ngl
        foreach (string propertyValue in a)
        {
            Enum.Parse(typeof(InputIdentifier), propertyValue);
            Debug.Log(propertyValue);
        }
    }

    public static List<string> GetChangedProperties(object obj1, object obj2)
    {
        var changedProperties = new List<string>();

        if (obj1 == null || obj2 == null)
        {
            return changedProperties.ToList();
        }

        if (obj1.GetType() != obj2.GetType())
        {
            return changedProperties.ToList();
        }

        var properties1 = obj1.GetType().GetProperties();

        foreach (var property in properties1)
        {
            var value1 = property.GetValue(obj1, null);
            var value2 = property.GetValue(obj2, null);

            if (value1 == null || value2 == null)
            {
                if (value1 != value2)
                {
                    changedProperties.Add(property.Name);
                }
            }
            else if (!value1.Equals(value2))
            {
                changedProperties.Add(property.Name);
            }
        }



        return changedProperties.ToList();
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
