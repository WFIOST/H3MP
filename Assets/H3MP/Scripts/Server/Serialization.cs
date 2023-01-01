using System;
using System.Collections.Generic;
using FistVR;
using HarmonyLib;
using Riptide;
using UnityEngine;
using OneOf;

namespace H3MP.Server;

public static class Serialization
{
    //For some reason the ancient C# version Unity uses HATES function specialisation extensions so we must provide the full name
    
    //Vector 3
    public static Message AddVector3(this Message msg, Vector3 vec) => msg.Add(vec.x).Add(vec.y).Add(vec.z);
    public static Vector3 GetVector3(this Message msg) =>
        new Vector3(x: msg.GetFloat(), y: msg.GetFloat(), z: msg.GetFloat());
    
    public static Message AddVector2(this Message msg, Vector2 vec) => msg.Add(vec.x).Add(vec.y);
    public static Vector2 GetVector2(this Message msg) => new Vector2(x: msg.GetFloat(), y: msg.GetFloat());
    //Quaternion
    public static Message AddQuaternion(this Message msg, Quaternion vec) => msg.Add(vec.x).Add(vec.y).Add(vec.z).Add(vec.w);
    public static Quaternion GetQuaternion(this Message msg) =>
        new Quaternion(x: msg.GetFloat(), y: msg.GetFloat(), z: msg.GetFloat(), w: msg.GetFloat());
    
    public struct ObjectKVP : IMessageSerializable
    {
        public int ID { get; private set; }
        public NetworkedObject Object { get; private set; }

        public ObjectKVP(int id, NetworkedObject obj)
        {
            ID = id;
            Object = obj;
        }
        public ObjectKVP(KeyValuePair<int, NetworkedObject> kvp)
        {
            ID = kvp.Key;
            Object = kvp.Value;
        }

        public void Serialize(Message message) => message.Add(ID).Add(Object);

        public void Deserialize(Message message)
        {
            ID = message.GetInt();
            Object = message.GetSerializable<NetworkedObject>();
        }
    }

    public static ObjectKVP[] ToSerialisable(this Dictionary<int, NetworkedObject> dict)
    {
        var objs = new ObjectKVP[dict.Count];

        var i = 0;
        foreach (KeyValuePair<int, NetworkedObject> kvp in dict) 
            objs[i++] = new ObjectKVP(kvp);
        
        return objs;
    }
    
    public static OneOf<bool, float, Vector2, Vector3, Quaternion> UpdateInput(ref HandInput input, InputIdentifier id, Message msg) => id switch {
            InputIdentifier.TriggerUp                      => input.TriggerUp = msg.GetBool(),
            InputIdentifier.TriggerDown                    => input.TriggerDown = msg.GetBool(),
            InputIdentifier.TriggerPressed                 => input.TriggerPressed = msg.GetBool(),
            InputIdentifier.TriggerFloat                   => input.TriggerFloat = msg.GetFloat(),
            InputIdentifier.TriggerTouchUp                 => input.TriggerTouchUp = msg.GetBool(),
            InputIdentifier.TriggerTouchDown               => input.TriggerTouchDown = msg.GetBool(),
            InputIdentifier.TriggerTouched                 => input.TriggerTouched = msg.GetBool(),
            InputIdentifier.GripUp                         => input.GripUp = msg.GetBool(),
            InputIdentifier.GripDown                       => input.GripDown = msg.GetBool(),
            InputIdentifier.GripPressed                    => input.GripPressed = msg.GetBool(),
            InputIdentifier.GripTouchUp                    => input.GripTouchUp = msg.GetBool(),
            InputIdentifier.GripTouchDown                  => input.GripTouchDown = msg.GetBool(),
            InputIdentifier.GripTouched                    => input.GripTouched = msg.GetBool(),
            InputIdentifier.TouchpadUp                     => input.TouchpadUp = msg.GetBool(),
            InputIdentifier.TouchpadDown                   => input.TouchpadDown = msg.GetBool(),
            InputIdentifier.TouchpadPressed                => input.TouchpadPressed = msg.GetBool(),
            InputIdentifier.TouchpadTouchUp                => input.TouchpadTouchUp = msg.GetBool(),
            InputIdentifier.TouchpadTouchDown              => input.TouchpadTouchDown = msg.GetBool(),
            InputIdentifier.TouchpadTouched                => input.TouchpadTouched = msg.GetBool(),
            InputIdentifier.TouchpadAxes                   => input.TouchpadAxes = msg.GetVector2(),
            InputIdentifier.TouchpadNorthUp                => input.TouchpadNorthUp = msg.GetBool(),
            InputIdentifier.TouchpadNorthDown              => input.TouchpadNorthDown = msg.GetBool(),
            InputIdentifier.TouchpadNorthPressed           => input.TouchpadNorthPressed = msg.GetBool(),
            InputIdentifier.TouchpadSouthUp                => input.TouchpadSouthUp = msg.GetBool(),
            InputIdentifier.TouchpadSouthDown              => input.TouchpadSouthDown = msg.GetBool(),
            InputIdentifier.TouchpadSouthPressed           => input.TouchpadSouthPressed = msg.GetBool(),
            InputIdentifier.TouchpadWestUp                 => input.TouchpadWestUp = msg.GetBool(),
            InputIdentifier.TouchpadWestDown               => input.TouchpadWestDown = msg.GetBool(),
            InputIdentifier.TouchpadWestPressed            => input.TouchpadWestPressed = msg.GetBool(),
            InputIdentifier.TouchpadEastUp                 => input.TouchpadEastUp = msg.GetBool(),
            InputIdentifier.TouchpadEastDown               => input.TouchpadEastDown = msg.GetBool(),
            InputIdentifier.TouchpadEastPressed            => input.TouchpadEastPressed = msg.GetBool(),
            InputIdentifier.TouchpadCenterUp               => input.TouchpadCenterUp = msg.GetBool(),
            InputIdentifier.TouchpadCenterDown             => input.TouchpadCenterDown = msg.GetBool(),
            InputIdentifier.TouchpadCenterPressed          => input.TouchpadCenterPressed = msg.GetBool(),
            InputIdentifier.BYButtonUp                     => input.BYButtonUp = msg.GetBool(),
            InputIdentifier.BYButtonDown                   => input.BYButtonDown = msg.GetBool(),
            InputIdentifier.BYButtonPressed                => input.BYButtonPressed = msg.GetBool(),
            InputIdentifier.AXButtonUp                     => input.AXButtonUp = msg.GetBool(),
            InputIdentifier.AXButtonDown                   => input.AXButtonDown = msg.GetBool(),
            InputIdentifier.AXButtonPressed                => input.AXButtonPressed = msg.GetBool(),
            InputIdentifier.Secondary2AxisInputUp          => input.Secondary2AxisInputUp = msg.GetBool(),
            InputIdentifier.Secondary2AxisInputDown        => input.Secondary2AxisInputDown = msg.GetBool(),
            InputIdentifier.Secondary2AxisInputPressed     => input.Secondary2AxisInputPressed = msg.GetBool(),
            InputIdentifier.Secondary2AxisInputTouchUp     => input.Secondary2AxisInputTouchUp = msg.GetBool(),
            InputIdentifier.Secondary2AxisInputTouchDown   => input.Secondary2AxisInputTouchDown = msg.GetBool(),
            InputIdentifier.Secondary2AxisInputTouched     => input.Secondary2AxisInputTouched = msg.GetBool(),
            InputIdentifier.Secondary2AxisInputAxes        => input.Secondary2AxisInputAxes = msg.GetVector2(),
            InputIdentifier.Secondary2AxisNorthUp          => input.Secondary2AxisNorthUp = msg.GetBool(),
            InputIdentifier.Secondary2AxisNorthDown        => input.Secondary2AxisNorthDown = msg.GetBool(),
            InputIdentifier.Secondary2AxisNorthPressed     => input.Secondary2AxisNorthPressed = msg.GetBool(),
            InputIdentifier.Secondary2AxisSouthUp          => input.Secondary2AxisSouthUp = msg.GetBool(),
            InputIdentifier.Secondary2AxisSouthDown        => input.Secondary2AxisSouthDown = msg.GetBool(),
            InputIdentifier.Secondary2AxisSouthPressed     => input.Secondary2AxisSouthPressed = msg.GetBool(),
            InputIdentifier.Secondary2AxisWestUp           => input.Secondary2AxisWestUp = msg.GetBool(),
            InputIdentifier.Secondary2AxisWestDown         => input.Secondary2AxisWestDown = msg.GetBool(),
            InputIdentifier.Secondary2AxisWestPressed      => input.Secondary2AxisWestPressed = msg.GetBool(),
            InputIdentifier.Secondary2AxisEastUp           => input.Secondary2AxisEastUp = msg.GetBool(),
            InputIdentifier.Secondary2AxisEastDown         => input.Secondary2AxisEastDown = msg.GetBool(),
            InputIdentifier.Secondary2AxisEastPressed      => input.Secondary2AxisEastPressed = msg.GetBool(),
            InputIdentifier.Secondary2AxisCenterUp         => input.Secondary2AxisCenterUp = msg.GetBool(),
            InputIdentifier.Secondary2AxisCenterDown       => input.Secondary2AxisCenterDown = msg.GetBool(),
            InputIdentifier.Secondary2AxisCenterPressed    => input.Secondary2AxisCenterPressed = msg.GetBool(),
            InputIdentifier.FingerCurl_Thumb               => input.FingerCurl_Thumb = msg.GetFloat(),
            InputIdentifier.FingerCurl_Index               => input.FingerCurl_Index = msg.GetFloat(),
            InputIdentifier.FingerCurl_Middle              => input.FingerCurl_Middle = msg.GetFloat(),
            InputIdentifier.FingerCurl_Ring                => input.FingerCurl_Ring = msg.GetFloat(),
            InputIdentifier.FingerCurl_Pinky               => input.FingerCurl_Pinky = msg.GetFloat(),
            InputIdentifier.LastCurlAverage                => input.LastCurlAverage = msg.GetFloat(),
            InputIdentifier.m_pos                          => input.m_pos = msg.GetVector3(),
            InputIdentifier.m_rot                          => input.m_rot = msg.GetQuaternion(),
            InputIdentifier.m_palmpos                      => input.m_palmpos = msg.GetVector3(),
            InputIdentifier.m_palmrot                      => input.m_palmrot = msg.GetQuaternion(),
            InputIdentifier.FilteredPos                    => input.FilteredPos = msg.GetVector3(),
            InputIdentifier.FilteredRot                    => input.FilteredRot = msg.GetQuaternion(),
            InputIdentifier.FilteredPalmPos                => input.FilteredPalmPos = msg.GetVector3(),
            InputIdentifier.FilteredPalmRot                => input.FilteredPalmRot = msg.GetQuaternion(),
            InputIdentifier.FilteredForward                => input.FilteredForward = msg.GetVector3(),
            InputIdentifier.FilteredPointingPos            => input.FilteredPointingPos = msg.GetVector3(),
            InputIdentifier.FilteredPointingForward        => input.FilteredPointingForward = msg.GetVector3(),
            InputIdentifier.m_up                           => input.m_up = msg.GetVector3(),
            InputIdentifier.m_right                        => input.m_right = msg.GetVector3(),
            InputIdentifier.m_forward                      => input.m_forward = msg.GetVector3(),
            InputIdentifier.FilteredUp                     => input.FilteredUp = msg.GetVector3(),
            InputIdentifier.FilteredRight                  => input.FilteredRight = msg.GetVector3(),
            InputIdentifier.VelLinearLocal                 => input.VelLinearLocal = msg.GetVector3(),
            InputIdentifier.VelAngularLocal                => input.VelAngularLocal = msg.GetVector3(),
            InputIdentifier.VelLinearWorld                 => input.VelLinearWorld = msg.GetVector3(),
            InputIdentifier.VelAngularWorld                => input.VelAngularWorld = msg.GetVector3(),
            InputIdentifier.IsGrabUp                       => input.IsGrabUp = msg.GetBool(),
            InputIdentifier.IsGrabDown                     => input.IsGrabDown = msg.GetBool(),
            InputIdentifier.IsGrabbing                     => input.IsGrabbing = msg.GetBool(),
            InputIdentifier.PosUltraFilter                 => input.PosUltraFilter = msg.GetVector3(),
            InputIdentifier.RotUltraFilter                 => input.RotUltraFilter = msg.GetQuaternion(),
            InputIdentifier.m_oneEuroLocalPosition         => input.m_oneEuroLocalPosition = msg.GetVector3(),
            InputIdentifier.m_oneEuroLocalPalmPosition     => input.m_oneEuroLocalPalmPosition = msg.GetVector3(),
            InputIdentifier.m_oneEuroLocalPointingPosition => input.m_oneEuroLocalPointingPosition = msg.GetVector3(),
            InputIdentifier.OneEuroRotation                => input.OneEuroRotation = msg.GetQuaternion(),
            InputIdentifier.OneEuroPalmRotation            => input.OneEuroPalmRotation = msg.GetQuaternion(),
            InputIdentifier.OneEuroPointRotation           => input.OneEuroPointRotation = msg.GetQuaternion(),
            InputIdentifier.LastPalmPos1                   => input.LastPalmPos1 = msg.GetVector3(),
            InputIdentifier.LastPalmPos2                   => input.LastPalmPos2 = msg.GetVector3(),
            InputIdentifier.Pos                            => input.Pos = msg.GetVector3(),
            InputIdentifier.Rot                            => input.Rot = msg.GetQuaternion(),
            InputIdentifier.PalmPos                        => input.PalmPos = msg.GetVector3(),
            InputIdentifier.PalmRot                        => input.PalmRot = msg.GetQuaternion(),
            _                                              => throw new ArgumentOutOfRangeException()
        };
}