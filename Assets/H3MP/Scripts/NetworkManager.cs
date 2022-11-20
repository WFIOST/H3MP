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

    private Player playerUpdateStruct;
    
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
        playerUpdateStruct.Head.position = GM.CurrentPlayerBody.Head.position;
        playerUpdateStruct.Head.rotation = GM.CurrentPlayerBody.Head.rotation;
        playerUpdateStruct.LeftHand.Transform.position = GM.CurrentMovementManager.Hands[0].transform.position;
        playerUpdateStruct.LeftHand.Transform.rotation = GM.CurrentMovementManager.Hands[0].transform.rotation;
        playerUpdateStruct.RightHand.Transform.position = GM.CurrentMovementManager.Hands[1].transform.position;
        playerUpdateStruct.RightHand.Transform.rotation = GM.CurrentMovementManager.Hands[1].transform.rotation;
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
        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort) MessageIdentifier.Player.UPDATE_TRANSFORM);

        msg.Add(playerUpdateStruct);
        _networking.Client.Send(msg);

        _networking.Client.Send(CreateInputMessage(_playerLeftHand));
        _networking.Client.Send(CreateInputMessage(_playerRightHand));
    }

    private Message CreateInputMessage(FVRViveHand hand)
    {
        var input = new SerialisableInput
        {
            Trigger = new SerialisableInput.LeverInput
            {
                Button = new SerialisableInput.ButtonInput
                {
                    Pressed = hand.Input.TriggerPressed,
                    Touched = hand.Input.TriggerTouched
                },
                Value = hand.Input.TriggerFloat
            },
            Touchpad = new SerialisableInput.TouchpadInput
            {
                Axes = hand.Input.TouchpadAxes,
                Button = new SerialisableInput.ButtonInput
                {
                    Pressed = hand.Input.TouchpadPressed,
                    Touched = hand.Input.TouchpadTouched
                }
            },
            Secondary2Axis = new SerialisableInput.TouchpadInput
            {
                Axes = hand.Input.Secondary2AxisInputAxes,
                Button = new SerialisableInput.ButtonInput
                {
                    Pressed = hand.Input.Secondary2AxisInputPressed,
                    Touched = hand.Input.Secondary2AxisInputTouched
                }
            },
            
            TouchpadNorth = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.TouchpadNorthPressed,
                Touched = hand.Input.TouchpadNorthDown
            },
            TouchpadSouth = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.TouchpadSouthPressed,
                Touched = hand.Input.TouchpadSouthDown
            },
            TouchpadWest = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.TouchpadWestPressed,
                Touched = hand.Input.TouchpadWestDown
            },
            TouchpadEast = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.TouchpadEastPressed,
                Touched = hand.Input.TouchpadEastDown
                
            },
            TouchpadCentre = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.TouchpadCenterPressed,
                Touched = hand.Input.TouchpadCenterDown
            },
            
            AX = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.AXButtonPressed,
                Touched = hand.Input.AXButtonDown
            },
            BY = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.BYButtonPressed,
                Touched = hand.Input.BYButtonDown
            },
            
            Secondary2AxisNorth = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.Secondary2AxisNorthPressed,
                Touched = hand.Input.Secondary2AxisNorthDown
            },
            Secondary2AxisSouth = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.Secondary2AxisSouthPressed,
                Touched = hand.Input.Secondary2AxisSouthDown
            },
            Secondary2AxisEast = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.Secondary2AxisEastPressed,
                Touched = hand.Input.Secondary2AxisEastDown
            },
            Secondary2AxisWest = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.Secondary2AxisWestPressed,
                Touched = hand.Input.Secondary2AxisWestDown
            },
            Secondary2AxisCentre = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.Secondary2AxisCenterPressed,
                Touched = hand.Input.Secondary2AxisCenterDown
            },
            
            Grip = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.GripPressed,
                Touched = hand.Input.GripTouched
            },
            Grabbing = new SerialisableInput.ButtonInput
            {
                Pressed = hand.Input.IsGrabbing,
                Touched = hand.Input.IsGrabDown
            },
            
            FromLeftHand = !hand.IsThisTheRightHand
        };
        
        return Message.Create(MessageSendMode.Unreliable, MessageIdentifier.Player.UPDATE_INPUT).Add(input);
    }

    private void HandleInput(Message msg)
    {
        var id = msg.GetUShort();
        var input = msg.GetSerializable<SerialisableInput>();

        scenePlayers.First(p => p.ID == id).InputUpdate(input);
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
