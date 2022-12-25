using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using H3MP.Core;
using System.Linq;
using FistVR;
using H3MP.Scripts;
using OneOf;
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
    

    // Previous hand inputs.
    private HandInput _LPrev = new HandInput();
    private HandInput _RPrev = new HandInput();

    private bool _firstRun = true;

    private KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>[] CheckInputStatus(bool Left)
    {
        if (_firstRun)
        {
            _LPrev = _playerLeftHand.Input;
            _RPrev = _playerRightHand.Input;
            _firstRun = false;
        }
        if (Left)
        {
            var _LCurrent = GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>().Input;
            var _LChanges = new List<KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>>();

            // Check for changes.
            if (_LCurrent.TriggerDown != _LPrev.TriggerDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerDown, _LCurrent.TriggerDown));
            if (_LCurrent.TriggerUp != _LPrev.TriggerUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerUp, _LCurrent.TriggerUp));
            if (_LCurrent.TriggerPressed != _LPrev.TriggerPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerPressed, _LCurrent.TriggerPressed));
            if (_LCurrent.TriggerFloat != _LPrev.TriggerFloat)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerFloat, _LCurrent.TriggerFloat));
            if (_LCurrent.AXButtonDown != _LPrev.AXButtonDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.AXButtonDown, _LCurrent.AXButtonDown));
            if (_LCurrent.AXButtonUp != _LPrev.AXButtonUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.AXButtonUp, _LCurrent.AXButtonUp));
            if (_LCurrent.AXButtonPressed != _LPrev.AXButtonPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.AXButtonPressed, _LCurrent.AXButtonPressed));
            if (_LCurrent.GripDown != _LPrev.GripDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripDown, _LCurrent.GripDown));
            if (_LCurrent.GripUp != _LPrev.GripUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripUp, _LCurrent.GripUp));
            if (_LCurrent.GripPressed != _LPrev.GripPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripPressed, _LCurrent.GripPressed));
            if (_LCurrent.GripTouchDown != _LPrev.GripTouchDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripTouchDown, _LCurrent.GripTouchDown));
            if (_LCurrent.GripTouchUp != _LPrev.GripTouchUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripTouchUp, _LCurrent.GripTouchUp));
            if (_LCurrent.GripTouched != _LPrev.GripTouched)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripTouched, _LCurrent.GripTouched));
            if (_LCurrent.BYButtonDown != _LPrev.BYButtonDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.BYButtonDown, _LCurrent.BYButtonDown));
            if (_LCurrent.BYButtonUp != _LPrev.BYButtonUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.BYButtonUp, _LCurrent.BYButtonUp));
            if (_LCurrent.BYButtonPressed != _LPrev.BYButtonPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.BYButtonPressed, _LCurrent.BYButtonPressed));
            if (_LCurrent.OneEuroPalmRotation != _LPrev.OneEuroPalmRotation)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.OneEuroPalmRotation, _LCurrent.OneEuroPalmRotation));
            if (_LCurrent.rotationUltraFilter != _LPrev.rotationUltraFilter)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.RotUltraFilter, _LCurrent.rotationUltraFilter.currValue));
            if (_LCurrent.VelLinearWorld != _LPrev.VelLinearWorld)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearWorld, _LCurrent.VelLinearWorld));
            if (_LCurrent.VelAngularWorld != _LPrev.VelAngularWorld)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularWorld, _LCurrent.VelAngularWorld));
            if (_LCurrent.VelLinearLocal != _LPrev.VelLinearLocal)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearLocal, _LCurrent.VelLinearLocal));
            if (_LCurrent.VelAngularLocal != _LPrev.VelAngularLocal)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularLocal, _LCurrent.VelAngularLocal));
            if (_LCurrent.FilteredForward != _LPrev.FilteredForward)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredForward, _LCurrent.FilteredForward));
            if (_LCurrent.FilteredUp != _LPrev.FilteredUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredUp, _LCurrent.FilteredUp));
            if (_LCurrent.FilteredRight != _LPrev.FilteredRight)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredRight, _LCurrent.FilteredRight));
            if (_LCurrent.FilteredPos != _LPrev.FilteredPos)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPos, _LCurrent.FilteredPos));
            if (_LCurrent.FilteredRot != _LPrev.FilteredRot)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredRot, _LCurrent.FilteredRot));
            if (_LCurrent.FilteredPalmRot != _LPrev.FilteredPalmRot)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPalmRot, _LCurrent.FilteredPalmRot));
            if (_LCurrent.FilteredPalmPos != _LPrev.FilteredPalmPos)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPalmPos, _LCurrent.FilteredPalmPos));
            if (_LCurrent.FilteredPointingForward != _LPrev.FilteredPointingForward)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPointingForward, _LCurrent.FilteredPointingForward));
            if (_LCurrent.FilteredPointingPos != _LPrev.FilteredPointingPos)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPointingPos, _LCurrent.FilteredPointingPos));
            if (_LCurrent.rotationFilterPalm != _LPrev.rotationFilterPalm)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.RotUltraFilter, _LCurrent.rotationFilterPalm.currValue));
            if (_LCurrent.FingerCurl_Index != _LPrev.FingerCurl_Index)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Index, _LCurrent.FingerCurl_Index));
            if (_LCurrent.FingerCurl_Middle != _LPrev.FingerCurl_Middle)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Middle, _LCurrent.FingerCurl_Middle));
            if (_LCurrent.FingerCurl_Ring != _LPrev.FingerCurl_Ring)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Ring, _LCurrent.FingerCurl_Ring));
            if (_LCurrent.FingerCurl_Pinky != _LPrev.FingerCurl_Pinky)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Pinky, _LCurrent.FingerCurl_Pinky));
            if (_LCurrent.FingerCurl_Thumb != _LPrev.FingerCurl_Thumb)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Thumb, _LCurrent.FingerCurl_Thumb));
            if (_LCurrent.IsGrabDown != _LPrev.IsGrabDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.IsGrabDown, _LCurrent.IsGrabDown));
            if (_LCurrent.IsGrabUp != _LPrev.IsGrabUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.IsGrabUp, _LCurrent.IsGrabUp));
            if (_LCurrent.IsGrabbing != _LPrev.IsGrabbing)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.IsGrabbing, _LCurrent.IsGrabbing));
            if (_LCurrent.LastCurlAverage != _LPrev.LastCurlAverage)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.LastCurlAverage, _LCurrent.LastCurlAverage));
            if (_LCurrent.LastPalmPos1 != _LPrev.LastPalmPos1)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.LastPalmPos1, _LCurrent.LastPalmPos1));
            if (_LCurrent.LastPalmPos2 != _LPrev.LastPalmPos2)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.LastPalmPos2, _LCurrent.LastPalmPos2));
            if (_LCurrent.OneEuroPalmPosition != _LPrev.OneEuroPalmPosition)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.m_oneEuroLocalPalmPosition, _LCurrent.OneEuroPalmPosition));
            if (_LCurrent.PalmPos != _LPrev.PalmPos)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.PalmPos, _LCurrent.PalmPos));
            if (_LCurrent.PalmRot != _LPrev.PalmRot)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.PalmRot, _LCurrent.PalmRot));
            if (_LCurrent.Rot != _LPrev.Rot)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Rot, _LCurrent.Rot));
            if (_LCurrent.RotUltraFilter != _LPrev.RotUltraFilter)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.RotUltraFilter, _LCurrent.RotUltraFilter));
            if (_LCurrent.Secondary2AxisCenterDown != _LPrev.Secondary2AxisCenterDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisCenterDown, _LCurrent.Secondary2AxisCenterDown));
            if (_LCurrent.Secondary2AxisCenterUp != _LPrev.Secondary2AxisCenterUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisCenterUp, _LCurrent.Secondary2AxisCenterUp));
            if (_LCurrent.Secondary2AxisEastDown != _LPrev.Secondary2AxisEastDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisEastDown, _LCurrent.Secondary2AxisEastDown));
            if (_LCurrent.Secondary2AxisEastUp != _LPrev.Secondary2AxisEastUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisEastUp, _LCurrent.Secondary2AxisEastUp));
            if (_LCurrent.Secondary2AxisNorthDown != _LPrev.Secondary2AxisNorthDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisNorthDown, _LCurrent.Secondary2AxisNorthDown));
            if (_LCurrent.Secondary2AxisNorthUp != _LPrev.Secondary2AxisNorthUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisNorthUp, _LCurrent.Secondary2AxisNorthUp));
            if (_LCurrent.Secondary2AxisSouthDown != _LPrev.Secondary2AxisSouthDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisSouthDown, _LCurrent.Secondary2AxisSouthDown));
            if (_LCurrent.Secondary2AxisSouthUp != _LPrev.Secondary2AxisSouthUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisSouthUp, _LCurrent.Secondary2AxisSouthUp));
            if (_LCurrent.Secondary2AxisWestDown != _LPrev.Secondary2AxisWestDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisWestDown, _LCurrent.Secondary2AxisWestDown));
            if (_LCurrent.Secondary2AxisWestUp != _LPrev.Secondary2AxisWestUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisWestUp, _LCurrent.Secondary2AxisWestUp));
            if (_LCurrent.Secondary2AxisCenterPressed != _LPrev.Secondary2AxisCenterPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisCenterPressed, _LCurrent.Secondary2AxisCenterPressed));
            if (_LCurrent.Secondary2AxisEastPressed != _LPrev.Secondary2AxisEastPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisEastPressed, _LCurrent.Secondary2AxisEastPressed));
            if (_LCurrent.Secondary2AxisNorthPressed != _LPrev.Secondary2AxisNorthPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisNorthPressed, _LCurrent.Secondary2AxisNorthPressed));
            if (_LCurrent.Secondary2AxisSouthPressed != _LPrev.Secondary2AxisSouthPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisSouthPressed, _LCurrent.Secondary2AxisSouthPressed));
            if (_LCurrent.Secondary2AxisWestPressed != _LPrev.Secondary2AxisWestPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisWestPressed, _LCurrent.Secondary2AxisWestPressed));
            if (_LCurrent.TouchpadAxes != _LPrev.TouchpadAxes)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadAxes, _LCurrent.TouchpadAxes));
            if (_LCurrent.TouchpadDown != _LPrev.TouchpadDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadDown, _LCurrent.TouchpadDown));
            if (_LCurrent.TouchpadPressed != _LPrev.TouchpadPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadPressed, _LCurrent.TouchpadPressed));
            if (_LCurrent.TouchpadUp != _LPrev.TouchpadUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadUp, _LCurrent.TouchpadUp));
            if (_LCurrent.TouchpadCenterDown != _LPrev.TouchpadCenterDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadCenterDown, _LCurrent.TouchpadCenterDown));
            if (_LCurrent.TouchpadCenterUp != _LPrev.TouchpadCenterUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadCenterUp, _LCurrent.TouchpadCenterUp));
            if (_LCurrent.TouchpadCenterPressed != _LPrev.TouchpadCenterPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadCenterPressed, _LCurrent.TouchpadCenterPressed));
            if (_LCurrent.TouchpadEastDown != _LPrev.TouchpadEastDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadEastDown, _LCurrent.TouchpadEastDown));
            if (_LCurrent.TouchpadEastUp != _LPrev.TouchpadEastUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadEastUp, _LCurrent.TouchpadEastUp));
            if (_LCurrent.TouchpadEastPressed != _LPrev.TouchpadEastPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadEastPressed, _LCurrent.TouchpadEastPressed));
            if (_LCurrent.TouchpadNorthDown != _LPrev.TouchpadNorthDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadNorthDown, _LCurrent.TouchpadNorthDown));
            if (_LCurrent.TouchpadNorthUp != _LPrev.TouchpadNorthUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadNorthUp, _LCurrent.TouchpadNorthUp));
            if (_LCurrent.TouchpadNorthPressed != _LPrev.TouchpadNorthPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadNorthPressed, _LCurrent.TouchpadNorthPressed));
            if (_LCurrent.TouchpadSouthDown != _LPrev.TouchpadSouthDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadSouthDown, _LCurrent.TouchpadSouthDown));
            if (_LCurrent.TouchpadSouthUp != _LPrev.TouchpadSouthUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadSouthUp, _LCurrent.TouchpadSouthUp));
            if (_LCurrent.TouchpadSouthPressed != _LPrev.TouchpadSouthPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadSouthPressed, _LCurrent.TouchpadSouthPressed));
            if (_LCurrent.TouchpadWestDown != _LPrev.TouchpadWestDown)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadWestDown, _LCurrent.TouchpadWestDown));
            if (_LCurrent.TouchpadWestUp != _LPrev.TouchpadWestUp)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadWestUp, _LCurrent.TouchpadWestUp));
            if (_LCurrent.TouchpadWestPressed != _LPrev.TouchpadWestPressed)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadWestPressed, _LCurrent.TouchpadWestPressed));
            if (_LCurrent.TouchpadTouched != _LPrev.TouchpadTouched)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadTouched, _LCurrent.TouchpadTouched));
            if (_LCurrent.VelAngularLocal != _LPrev.VelAngularLocal)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularLocal, _LCurrent.VelAngularLocal));
            if (_LCurrent.VelAngularWorld != _LPrev.VelAngularWorld)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularWorld, _LCurrent.VelAngularWorld));
            if (_LCurrent.VelLinearLocal != _LPrev.VelLinearLocal)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearLocal, _LCurrent.VelLinearLocal));
            if (_LCurrent.VelLinearWorld != _LPrev.VelLinearWorld)
                _LChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearWorld, _LCurrent.VelLinearWorld));

            return _LChanges.ToArray();
        }
        else
        {
            var _RCurrent = GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>().Input;
            var _RChanges = new List<KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>>();
            // Check for changes.
            if (_RCurrent.TriggerDown != _LPrev.TriggerDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerDown, _RCurrent.TriggerDown));
            if (_RCurrent.TriggerUp != _LPrev.TriggerUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerUp, _RCurrent.TriggerUp));
            if (_RCurrent.TriggerPressed != _LPrev.TriggerPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerPressed, _RCurrent.TriggerPressed));
            if (_RCurrent.TriggerFloat != _LPrev.TriggerFloat)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TriggerFloat, _RCurrent.TriggerFloat));
            if (_RCurrent.AXButtonDown != _LPrev.AXButtonDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.AXButtonDown, _RCurrent.AXButtonDown));
            if (_RCurrent.AXButtonUp != _LPrev.AXButtonUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.AXButtonUp, _RCurrent.AXButtonUp));
            if (_RCurrent.AXButtonPressed != _LPrev.AXButtonPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.AXButtonPressed, _RCurrent.AXButtonPressed));
            if (_RCurrent.GripDown != _LPrev.GripDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripDown, _RCurrent.GripDown));
            if (_RCurrent.GripUp != _LPrev.GripUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripUp, _RCurrent.GripUp));
            if (_RCurrent.GripPressed != _LPrev.GripPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripPressed, _RCurrent.GripPressed));
            if (_RCurrent.GripTouchDown != _LPrev.GripTouchDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripTouchDown, _RCurrent.GripTouchDown));
            if (_RCurrent.GripTouchUp != _LPrev.GripTouchUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripTouchUp, _RCurrent.GripTouchUp));
            if (_RCurrent.GripTouched != _LPrev.GripTouched)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.GripTouched, _RCurrent.GripTouched));
            if (_RCurrent.BYButtonDown != _LPrev.BYButtonDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.BYButtonDown, _RCurrent.BYButtonDown));
            if (_RCurrent.BYButtonUp != _LPrev.BYButtonUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.BYButtonUp, _RCurrent.BYButtonUp));
            if (_RCurrent.BYButtonPressed != _LPrev.BYButtonPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.BYButtonPressed, _RCurrent.BYButtonPressed));
            if (_RCurrent.OneEuroPalmRotation != _LPrev.OneEuroPalmRotation)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.OneEuroPalmRotation, _RCurrent.OneEuroPalmRotation));
            if (_RCurrent.rotationUltraFilter != _LPrev.rotationUltraFilter)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.RotUltraFilter, _RCurrent.rotationUltraFilter.currValue));
            if (_RCurrent.VelLinearWorld != _LPrev.VelLinearWorld)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearWorld, _RCurrent.VelLinearWorld));
            if (_RCurrent.VelAngularWorld != _LPrev.VelAngularWorld)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularWorld, _RCurrent.VelAngularWorld));
            if (_RCurrent.VelLinearLocal != _LPrev.VelLinearLocal)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearLocal, _RCurrent.VelLinearLocal));
            if (_RCurrent.VelAngularLocal != _LPrev.VelAngularLocal)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularLocal, _RCurrent.VelAngularLocal));
            if (_RCurrent.FilteredForward != _LPrev.FilteredForward)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredForward, _RCurrent.FilteredForward));
            if (_RCurrent.FilteredUp != _LPrev.FilteredUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredUp, _RCurrent.FilteredUp));
            if (_RCurrent.FilteredRight != _LPrev.FilteredRight)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredRight, _RCurrent.FilteredRight));
            if (_RCurrent.FilteredPos != _LPrev.FilteredPos)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPos, _RCurrent.FilteredPos));
            if (_RCurrent.FilteredRot != _LPrev.FilteredRot)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredRot, _RCurrent.FilteredRot));
            if (_RCurrent.FilteredPalmRot != _LPrev.FilteredPalmRot)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPalmRot, _RCurrent.FilteredPalmRot));
            if (_RCurrent.FilteredPalmPos != _LPrev.FilteredPalmPos)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPalmPos, _RCurrent.FilteredPalmPos));
            if (_RCurrent.FilteredPointingForward != _LPrev.FilteredPointingForward)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPointingForward, _RCurrent.FilteredPointingForward));
            if (_RCurrent.FilteredPointingPos != _LPrev.FilteredPointingPos)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FilteredPointingPos, _RCurrent.FilteredPointingPos));
            if (_RCurrent.rotationFilterPalm != _LPrev.rotationFilterPalm)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.RotUltraFilter, _RCurrent.rotationFilterPalm.currValue));
            if (_RCurrent.FingerCurl_Index != _LPrev.FingerCurl_Index)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Index, _RCurrent.FingerCurl_Index));
            if (_RCurrent.FingerCurl_Middle != _LPrev.FingerCurl_Middle)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Middle, _RCurrent.FingerCurl_Middle));
            if (_RCurrent.FingerCurl_Ring != _LPrev.FingerCurl_Ring)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Ring, _RCurrent.FingerCurl_Ring));
            if (_RCurrent.FingerCurl_Pinky != _LPrev.FingerCurl_Pinky)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Pinky, _RCurrent.FingerCurl_Pinky));
            if (_RCurrent.FingerCurl_Thumb != _LPrev.FingerCurl_Thumb)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.FingerCurl_Thumb, _RCurrent.FingerCurl_Thumb));
            if (_RCurrent.IsGrabDown != _LPrev.IsGrabDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.IsGrabDown, _RCurrent.IsGrabDown));
            if (_RCurrent.IsGrabUp != _LPrev.IsGrabUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.IsGrabUp, _RCurrent.IsGrabUp));
            if (_RCurrent.IsGrabbing != _LPrev.IsGrabbing)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.IsGrabbing, _RCurrent.IsGrabbing));
            if (_RCurrent.LastCurlAverage != _LPrev.LastCurlAverage)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.LastCurlAverage, _RCurrent.LastCurlAverage));
            if (_RCurrent.LastPalmPos1 != _LPrev.LastPalmPos1)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.LastPalmPos1, _RCurrent.LastPalmPos1));
            if (_RCurrent.LastPalmPos2 != _LPrev.LastPalmPos2)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.LastPalmPos2, _RCurrent.LastPalmPos2));
            if (_RCurrent.OneEuroPalmPosition != _LPrev.OneEuroPalmPosition)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.m_oneEuroLocalPalmPosition, _RCurrent.OneEuroPalmPosition));
            if (_RCurrent.PalmPos != _LPrev.PalmPos)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.PalmPos, _RCurrent.PalmPos));
            if (_RCurrent.PalmRot != _LPrev.PalmRot)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.PalmRot, _RCurrent.PalmRot));
            if (_RCurrent.Rot != _LPrev.Rot)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Rot, _RCurrent.Rot));
            if (_RCurrent.RotUltraFilter != _LPrev.RotUltraFilter)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.RotUltraFilter, _RCurrent.RotUltraFilter));
            if (_RCurrent.Secondary2AxisCenterDown != _LPrev.Secondary2AxisCenterDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisCenterDown, _RCurrent.Secondary2AxisCenterDown));
            if (_RCurrent.Secondary2AxisCenterUp != _LPrev.Secondary2AxisCenterUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisCenterUp, _RCurrent.Secondary2AxisCenterUp));
            if (_RCurrent.Secondary2AxisEastDown != _LPrev.Secondary2AxisEastDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisEastDown, _RCurrent.Secondary2AxisEastDown));
            if (_RCurrent.Secondary2AxisEastUp != _LPrev.Secondary2AxisEastUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisEastUp, _RCurrent.Secondary2AxisEastUp));
            if (_RCurrent.Secondary2AxisNorthDown != _LPrev.Secondary2AxisNorthDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisNorthDown, _RCurrent.Secondary2AxisNorthDown));
            if (_RCurrent.Secondary2AxisNorthUp != _LPrev.Secondary2AxisNorthUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisNorthUp, _RCurrent.Secondary2AxisNorthUp));
            if (_RCurrent.Secondary2AxisSouthDown != _LPrev.Secondary2AxisSouthDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisSouthDown, _RCurrent.Secondary2AxisSouthDown));
            if (_RCurrent.Secondary2AxisSouthUp != _LPrev.Secondary2AxisSouthUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisSouthUp, _RCurrent.Secondary2AxisSouthUp));
            if (_RCurrent.Secondary2AxisWestDown != _LPrev.Secondary2AxisWestDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisWestDown, _RCurrent.Secondary2AxisWestDown));
            if (_RCurrent.Secondary2AxisWestUp != _LPrev.Secondary2AxisWestUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisWestUp, _RCurrent.Secondary2AxisWestUp));
            if (_RCurrent.Secondary2AxisCenterPressed != _LPrev.Secondary2AxisCenterPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisCenterPressed, _RCurrent.Secondary2AxisCenterPressed));
            if (_RCurrent.Secondary2AxisEastPressed != _LPrev.Secondary2AxisEastPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisEastPressed, _RCurrent.Secondary2AxisEastPressed));
            if (_RCurrent.Secondary2AxisNorthPressed != _LPrev.Secondary2AxisNorthPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisNorthPressed, _RCurrent.Secondary2AxisNorthPressed));
            if (_RCurrent.Secondary2AxisSouthPressed != _LPrev.Secondary2AxisSouthPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisSouthPressed, _RCurrent.Secondary2AxisSouthPressed));
            if (_RCurrent.Secondary2AxisWestPressed != _LPrev.Secondary2AxisWestPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.Secondary2AxisWestPressed, _RCurrent.Secondary2AxisWestPressed));
            if (_RCurrent.TouchpadAxes != _LPrev.TouchpadAxes)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadAxes, _RCurrent.TouchpadAxes));
            if (_RCurrent.TouchpadDown != _LPrev.TouchpadDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadDown, _RCurrent.TouchpadDown));
            if (_RCurrent.TouchpadPressed != _LPrev.TouchpadPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadPressed, _RCurrent.TouchpadPressed));
            if (_RCurrent.TouchpadUp != _LPrev.TouchpadUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadUp, _RCurrent.TouchpadUp));
            if (_RCurrent.TouchpadCenterDown != _LPrev.TouchpadCenterDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadCenterDown, _RCurrent.TouchpadCenterDown));
            if (_RCurrent.TouchpadCenterUp != _LPrev.TouchpadCenterUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadCenterUp, _RCurrent.TouchpadCenterUp));
            if (_RCurrent.TouchpadCenterPressed != _LPrev.TouchpadCenterPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadCenterPressed, _RCurrent.TouchpadCenterPressed));
            if (_RCurrent.TouchpadEastDown != _LPrev.TouchpadEastDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadEastDown, _RCurrent.TouchpadEastDown));
            if (_RCurrent.TouchpadEastUp != _LPrev.TouchpadEastUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadEastUp, _RCurrent.TouchpadEastUp));
            if (_RCurrent.TouchpadEastPressed != _LPrev.TouchpadEastPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadEastPressed, _RCurrent.TouchpadEastPressed));
            if (_RCurrent.TouchpadNorthDown != _LPrev.TouchpadNorthDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadNorthDown, _RCurrent.TouchpadNorthDown));
            if (_RCurrent.TouchpadNorthUp != _LPrev.TouchpadNorthUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadNorthUp, _RCurrent.TouchpadNorthUp));
            if (_RCurrent.TouchpadNorthPressed != _LPrev.TouchpadNorthPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadNorthPressed, _RCurrent.TouchpadNorthPressed));
            if (_RCurrent.TouchpadSouthDown != _LPrev.TouchpadSouthDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadSouthDown, _RCurrent.TouchpadSouthDown));
            if (_RCurrent.TouchpadSouthUp != _LPrev.TouchpadSouthUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadSouthUp, _RCurrent.TouchpadSouthUp));
            if (_RCurrent.TouchpadSouthPressed != _LPrev.TouchpadSouthPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadSouthPressed, _RCurrent.TouchpadSouthPressed));
            if (_RCurrent.TouchpadWestDown != _LPrev.TouchpadWestDown)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadWestDown, _RCurrent.TouchpadWestDown));
            if (_RCurrent.TouchpadWestUp != _LPrev.TouchpadWestUp)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadWestUp, _RCurrent.TouchpadWestUp));
            if (_RCurrent.TouchpadWestPressed != _LPrev.TouchpadWestPressed)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadWestPressed, _RCurrent.TouchpadWestPressed));
            if (_RCurrent.TouchpadTouched != _LPrev.TouchpadTouched)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.TouchpadTouched, _RCurrent.TouchpadTouched));
            if (_RCurrent.VelAngularLocal != _LPrev.VelAngularLocal)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularLocal, _RCurrent.VelAngularLocal));
            if (_RCurrent.VelAngularWorld != _LPrev.VelAngularWorld)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelAngularWorld, _RCurrent.VelAngularWorld));
            if (_RCurrent.VelLinearLocal != _LPrev.VelLinearLocal)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearLocal, _RCurrent.VelLinearLocal));
            if (_RCurrent.VelLinearWorld != _LPrev.VelLinearWorld)
                _RChanges.Add(new KeyValuePair<InputIdentifier, OneOf<bool, float, Vector2, Vector3, Quaternion>>(InputIdentifier.VelLinearWorld, _RCurrent.VelLinearWorld));

            return _RChanges.ToArray();
        }
    }

    private void HandleInput(Message msg)
    {
        //var plrid = msg.GetUShort();
        //byte rawid = msg.GetByte();
        //bool isRightHand = (rawid & 0b10000000) == 0;
        ////var inputID = (InputIdentifier)(isRightHand ? (rawid ^ 0b10000000) : rawid);

        SerialisableInput input = msg.GetSerializable<SerialisableInput>();
        input.UpdateInput(msg);
        //input.IsRightHand = isRightHand;

        //scenePlayers.Find(x => x.ID == plrid).InputUpdate(input);

        
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