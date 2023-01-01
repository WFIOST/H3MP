using System;
using System.Collections.Generic;
using FistVR;
using HarmonyLib;
using Riptide;
using UnityEngine;
using OneOf;

namespace H3MP.Server
{

    public static class Serialization
    {
        //For some reason the ancient C# version Unity uses HATES function specialisation extensions so we must provide the full name

        //Vector 3
        public static Message AddVector3(this Message msg, Vector3 vec)
        {
            return msg.Add(vec.x).Add(vec.y).Add(vec.z);
        }

        public static Vector3 GetVector3(this Message msg)
        {
            return new Vector3(x: msg.GetFloat(), y: msg.GetFloat(), z: msg.GetFloat());
        }

        public static Message AddVector2(this Message msg, Vector2 vec)
        {
            return msg.Add(vec.x).Add(vec.y);
        }

        public static Vector2 GetVector2(this Message msg)
        {
            return new Vector2(x: msg.GetFloat(), y: msg.GetFloat());
        }

        //Quaternion
        public static Message AddQuaternion(this Message msg, Quaternion vec)
        {
            return msg.Add(vec.x).Add(vec.y).Add(vec.z).Add(vec.w);
        }

        public static Quaternion GetQuaternion(this Message msg)
        {
            return new Quaternion(x: msg.GetFloat(), y: msg.GetFloat(), z: msg.GetFloat(), w: msg.GetFloat());
        }

        
        public struct ObjectKVP : IMessageSerializable
        {
            public int ID;
            public NetworkedObject Object;
            

            public ObjectKVP(KeyValuePair<int, NetworkedObject> kvp)
            {
                ID = kvp.Key;
                Object = kvp.Value;
            }

            public void Serialize(Message message)
            {
                message.Add(ID).Add(Object);
            }

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

        public static OneOf<bool, float, Vector2, Vector3, Quaternion> UpdateInput(ref HandInput input, InputIdentifier id, Message msg)
        {
            switch (id)
            {
                case InputIdentifier.TriggerUp:
                    return input.TriggerUp = msg.GetBool();
                case InputIdentifier.TriggerDown:
                    return input.TriggerDown = msg.GetBool();
                case InputIdentifier.TriggerPressed:
                    return input.TriggerPressed = msg.GetBool();
                case InputIdentifier.TriggerFloat:
                    return input.TriggerFloat = msg.GetFloat();
                case InputIdentifier.TriggerTouchUp:
                    return input.TriggerTouchUp = msg.GetBool();
                case InputIdentifier.TriggerTouchDown:
                    return input.TriggerTouchDown = msg.GetBool();
                case InputIdentifier.TriggerTouched:
                    return input.TriggerTouched = msg.GetBool();
                case InputIdentifier.GripUp:
                    return input.GripUp = msg.GetBool();
                case InputIdentifier.GripDown:
                    return input.GripDown = msg.GetBool();
                case InputIdentifier.GripPressed:
                    return input.GripPressed = msg.GetBool();
                case InputIdentifier.GripTouchUp:
                    return input.GripTouchUp = msg.GetBool();
                case InputIdentifier.GripTouchDown:
                    return input.GripTouchDown = msg.GetBool();
                case InputIdentifier.GripTouched:
                    return input.GripTouched = msg.GetBool();
                case InputIdentifier.TouchpadUp:
                    return input.TouchpadUp = msg.GetBool();
                case InputIdentifier.TouchpadDown:
                    return input.TouchpadDown = msg.GetBool();
                case InputIdentifier.TouchpadPressed:
                    return input.TouchpadPressed = msg.GetBool();
                case InputIdentifier.TouchpadTouchUp:
                    return input.TouchpadTouchUp = msg.GetBool();
                case InputIdentifier.TouchpadTouchDown:
                    return input.TouchpadTouchDown = msg.GetBool();
                case InputIdentifier.TouchpadTouched:
                    return input.TouchpadTouched = msg.GetBool();
                case InputIdentifier.TouchpadAxes:
                    return input.TouchpadAxes = msg.GetVector2();
                case InputIdentifier.TouchpadNorthUp:
                    return input.TouchpadNorthUp = msg.GetBool();
                case InputIdentifier.TouchpadNorthDown:
                    return input.TouchpadNorthDown = msg.GetBool();
                case InputIdentifier.TouchpadNorthPressed:
                    return input.TouchpadNorthPressed = msg.GetBool();
                case InputIdentifier.TouchpadSouthUp:
                    return input.TouchpadSouthUp = msg.GetBool();
                case InputIdentifier.TouchpadSouthDown:
                    return input.TouchpadSouthDown = msg.GetBool();
                case InputIdentifier.TouchpadSouthPressed:
                    return input.TouchpadSouthPressed = msg.GetBool();
                case InputIdentifier.TouchpadWestUp:
                    return input.TouchpadWestUp = msg.GetBool();
                case InputIdentifier.TouchpadWestDown:
                    return input.TouchpadWestDown = msg.GetBool();
                case InputIdentifier.TouchpadWestPressed:
                    return input.TouchpadWestPressed = msg.GetBool();
                case InputIdentifier.TouchpadEastUp:
                    return input.TouchpadEastUp = msg.GetBool();
                case InputIdentifier.TouchpadEastDown:
                    return input.TouchpadEastDown = msg.GetBool();
                case InputIdentifier.TouchpadEastPressed:
                    return input.TouchpadEastPressed = msg.GetBool();
                case InputIdentifier.TouchpadCenterUp:
                    return input.TouchpadCenterUp = msg.GetBool();
                case InputIdentifier.TouchpadCenterDown:
                    return input.TouchpadCenterDown = msg.GetBool();
                case InputIdentifier.TouchpadCenterPressed:
                    return input.TouchpadCenterPressed = msg.GetBool();
                case InputIdentifier.BYButtonUp:
                    return input.BYButtonUp = msg.GetBool();
                case InputIdentifier.BYButtonDown:
                    return input.BYButtonDown = msg.GetBool();
                case InputIdentifier.BYButtonPressed:
                    return input.BYButtonPressed = msg.GetBool();
                case InputIdentifier.AXButtonUp:
                    return input.AXButtonUp = msg.GetBool();
                case InputIdentifier.AXButtonDown:
                    return input.AXButtonDown = msg.GetBool();
                case InputIdentifier.AXButtonPressed:
                    return input.AXButtonPressed = msg.GetBool();
                case InputIdentifier.Secondary2AxisInputUp:
                    return input.Secondary2AxisInputUp = msg.GetBool();
                case InputIdentifier.Secondary2AxisInputDown:
                    return input.Secondary2AxisInputDown = msg.GetBool();
                case InputIdentifier.Secondary2AxisInputPressed:
                    return input.Secondary2AxisInputPressed = msg.GetBool();
                case InputIdentifier.Secondary2AxisInputTouchUp:
                    return input.Secondary2AxisInputTouchUp = msg.GetBool();
                case InputIdentifier.Secondary2AxisInputTouchDown:
                    return input.Secondary2AxisInputTouchDown = msg.GetBool();
                case InputIdentifier.Secondary2AxisInputTouched:
                    return input.Secondary2AxisInputTouched = msg.GetBool();
                case InputIdentifier.Secondary2AxisInputAxes:
                    return input.Secondary2AxisInputAxes = msg.GetVector2();
                case InputIdentifier.Secondary2AxisNorthUp:
                    return input.Secondary2AxisNorthUp = msg.GetBool();
                case InputIdentifier.Secondary2AxisNorthDown:
                    return input.Secondary2AxisNorthDown = msg.GetBool();
                case InputIdentifier.Secondary2AxisNorthPressed:
                    return input.Secondary2AxisNorthPressed = msg.GetBool();
                case InputIdentifier.Secondary2AxisSouthUp:
                    return input.Secondary2AxisSouthUp = msg.GetBool();
                case InputIdentifier.Secondary2AxisSouthDown:
                    return input.Secondary2AxisSouthDown = msg.GetBool();
                case InputIdentifier.Secondary2AxisSouthPressed:
                    return input.Secondary2AxisSouthPressed = msg.GetBool();
                case InputIdentifier.Secondary2AxisWestUp:
                    return input.Secondary2AxisWestUp = msg.GetBool();
                case InputIdentifier.Secondary2AxisWestDown:
                    return input.Secondary2AxisWestDown = msg.GetBool();
                case InputIdentifier.Secondary2AxisWestPressed:
                    return input.Secondary2AxisWestPressed = msg.GetBool();
                case InputIdentifier.Secondary2AxisEastUp:
                    return input.Secondary2AxisEastUp = msg.GetBool();
                case InputIdentifier.Secondary2AxisEastDown:
                    return input.Secondary2AxisEastDown = msg.GetBool();
                case InputIdentifier.Secondary2AxisEastPressed:
                    return input.Secondary2AxisEastPressed = msg.GetBool();
                case InputIdentifier.Secondary2AxisCenterUp:
                    return input.Secondary2AxisCenterUp = msg.GetBool();
                case InputIdentifier.Secondary2AxisCenterDown:
                    return input.Secondary2AxisCenterDown = msg.GetBool();
                case InputIdentifier.Secondary2AxisCenterPressed:
                    return input.Secondary2AxisCenterPressed = msg.GetBool();
                case InputIdentifier.FingerCurl_Thumb:
                    return input.FingerCurl_Thumb = msg.GetFloat();
                case InputIdentifier.FingerCurl_Index:
                    return input.FingerCurl_Index = msg.GetFloat();
                case InputIdentifier.FingerCurl_Middle:
                    return input.FingerCurl_Middle = msg.GetFloat();
                case InputIdentifier.FingerCurl_Ring:
                    return input.FingerCurl_Ring = msg.GetFloat();
                case InputIdentifier.FingerCurl_Pinky:
                    return input.FingerCurl_Pinky = msg.GetFloat();
                case InputIdentifier.LastCurlAverage:
                    return input.LastCurlAverage = msg.GetFloat();
                case InputIdentifier.m_pos:
                    return input.m_pos = msg.GetVector3();
                case InputIdentifier.m_rot:
                    return input.m_rot = msg.GetQuaternion();
                case InputIdentifier.m_palmpos:
                    return input.m_palmpos = msg.GetVector3();
                case InputIdentifier.m_palmrot:
                    return input.m_palmrot = msg.GetQuaternion();
                case InputIdentifier.FilteredPos:
                    return input.FilteredPos = msg.GetVector3();
                case InputIdentifier.FilteredRot:
                    return input.FilteredRot = msg.GetQuaternion();
                case InputIdentifier.FilteredPalmPos:
                    return input.FilteredPalmPos = msg.GetVector3();
                case InputIdentifier.FilteredPalmRot:
                    return input.FilteredPalmRot = msg.GetQuaternion();
                case InputIdentifier.FilteredForward:
                    return input.FilteredForward = msg.GetVector3();
                case InputIdentifier.FilteredPointingPos:
                    return input.FilteredPointingPos = msg.GetVector3();
                case InputIdentifier.FilteredPointingForward:
                    return input.FilteredPointingForward = msg.GetVector3();
                case InputIdentifier.m_up:
                    return input.m_up = msg.GetVector3();
                case InputIdentifier.m_right:
                    return input.m_right = msg.GetVector3();
                case InputIdentifier.m_forward:
                    return input.m_forward = msg.GetVector3();
                case InputIdentifier.FilteredUp:
                    return input.FilteredUp = msg.GetVector3();
                case InputIdentifier.FilteredRight:
                    return input.FilteredRight = msg.GetVector3();
                case InputIdentifier.VelLinearLocal:
                    return input.VelLinearLocal = msg.GetVector3();
                case InputIdentifier.VelAngularLocal:
                    return input.VelAngularLocal = msg.GetVector3();
                case InputIdentifier.VelLinearWorld:
                    return input.VelLinearWorld = msg.GetVector3();
                case InputIdentifier.VelAngularWorld:
                    return input.VelAngularWorld = msg.GetVector3();
                case InputIdentifier.IsGrabUp:
                    return input.IsGrabUp = msg.GetBool();
                case InputIdentifier.IsGrabDown:
                    return input.IsGrabDown = msg.GetBool();
                case InputIdentifier.IsGrabbing:
                    return input.IsGrabbing = msg.GetBool();
                case InputIdentifier.PosUltraFilter:
                    return input.PosUltraFilter = msg.GetVector3();
                case InputIdentifier.RotUltraFilter:
                    return input.RotUltraFilter = msg.GetQuaternion();
                case InputIdentifier.m_oneEuroLocalPosition:
                    return input.m_oneEuroLocalPosition = msg.GetVector3();
                case InputIdentifier.m_oneEuroLocalPalmPosition:
                    return input.m_oneEuroLocalPalmPosition = msg.GetVector3();
                case InputIdentifier.m_oneEuroLocalPointingPosition:
                    return input.m_oneEuroLocalPointingPosition = msg.GetVector3();
                case InputIdentifier.OneEuroRotation:
                    return input.OneEuroRotation = msg.GetQuaternion();
                case InputIdentifier.OneEuroPalmRotation:
                    return input.OneEuroPalmRotation = msg.GetQuaternion();
                case InputIdentifier.OneEuroPointRotation:
                    return input.OneEuroPointRotation = msg.GetQuaternion();
                case InputIdentifier.LastPalmPos1:
                    return input.LastPalmPos1 = msg.GetVector3();
                case InputIdentifier.LastPalmPos2:
                    return input.LastPalmPos2 = msg.GetVector3();
                case InputIdentifier.Pos:
                    return input.Pos = msg.GetVector3();
                case InputIdentifier.Rot:
                    return input.Rot = msg.GetQuaternion();
                case InputIdentifier.PalmPos:
                    return input.PalmPos = msg.GetVector3();
                case InputIdentifier.PalmRot:
                    return input.PalmRot = msg.GetQuaternion();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
