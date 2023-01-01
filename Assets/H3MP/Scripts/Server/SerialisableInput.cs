using System;
using FistVR;
using OneOf;
using Riptide;
using UnityEngine;

namespace H3MP.Server
{

    [Serializable]
    public class SerialisableInput : IMessageSerializable
    {
        public HandInput Input;
        public bool IsRightHand;

        public SerialisableInput() { }

        public SerialisableInput(FVRViveHand hand)
        {
            IsRightHand = hand.IsThisTheRightHand;
            Input = hand.Input;
        }

        public OneOf<bool, float, Vector2, Vector3, Quaternion> UpdateInput(InputIdentifier id, Message msg)
        {
            return Serialization.UpdateInput(input: ref Input, id: id, msg: msg);
        }

        public void Serialize(Message msg)
        {
            msg
                .Add(Input.TriggerUp).Add(Input.TriggerDown).Add(Input.TriggerPressed).Add(Input.TriggerFloat)
                .Add(Input.TriggerTouchUp).Add(Input.TriggerTouchDown).Add(Input.TriggerTouched)
                .Add(Input.GripUp).Add(Input.GripDown).Add(Input.GripPressed)
                .Add(Input.GripTouchUp).Add(Input.GripTouchDown).Add(Input.GripTouched)
                .Add(Input.TouchpadUp).Add(Input.TouchpadDown).Add(Input.TouchpadTouched).AddVector2(Input.TouchpadAxes)
                .Add(Input.TouchpadNorthUp).Add(Input.TouchpadNorthDown).Add(Input.TouchpadNorthPressed)
                .Add(Input.TouchpadSouthUp).Add(Input.TouchpadSouthDown).Add(Input.TouchpadSouthPressed)
                .Add(Input.TouchpadWestUp).Add(Input.TouchpadWestDown).Add(Input.TouchpadWestPressed)
                .Add(Input.TouchpadEastUp).Add(Input.TouchpadEastDown).Add(Input.TouchpadEastPressed)
                .Add(Input.TouchpadCenterUp).Add(Input.TouchpadCenterDown).Add(Input.TouchpadCenterPressed)
                .Add(Input.BYButtonUp).Add(Input.BYButtonDown).Add(Input.BYButtonPressed)
                .Add(Input.AXButtonUp).Add(Input.AXButtonDown).Add(Input.AXButtonPressed)
                .Add(Input.Secondary2AxisInputUp).Add(Input.Secondary2AxisInputDown).Add(Input.Secondary2AxisInputPressed)
                .Add(Input.Secondary2AxisInputTouchUp).Add(Input.Secondary2AxisInputTouchDown).Add(Input.Secondary2AxisInputTouched).AddVector2(Input.Secondary2AxisInputAxes)
                .Add(Input.Secondary2AxisNorthUp).Add(Input.Secondary2AxisNorthDown).Add(Input.Secondary2AxisNorthPressed)
                .Add(Input.Secondary2AxisSouthUp).Add(Input.Secondary2AxisSouthDown).Add(Input.Secondary2AxisSouthPressed)
                .Add(Input.Secondary2AxisWestUp).Add(Input.Secondary2AxisWestDown).Add(Input.Secondary2AxisWestPressed)
                .Add(Input.Secondary2AxisEastUp).Add(Input.Secondary2AxisEastDown).Add(Input.Secondary2AxisEastPressed)
                .Add(Input.Secondary2AxisCenterUp).Add(Input.Secondary2AxisCenterDown).Add(Input.Secondary2AxisCenterPressed)
                .Add(Input.FingerCurl_Thumb).Add(Input.FingerCurl_Index).Add(Input.FingerCurl_Middle).Add(Input.FingerCurl_Ring).Add(Input.FingerCurl_Pinky).Add(Input.LastCurlAverage)
                .AddVector3(Input.FilteredForward).AddVector3(Input.FilteredPos).AddVector3(Input.FilteredPointingForward)
                .AddVector3(Input.m_pos).AddQuaternion(Input.m_rot).AddVector3(Input.m_palmpos).AddQuaternion(Input.m_palmrot)
                .AddVector3(Input.FilteredPos).AddQuaternion(Input.FilteredRot).AddVector3(Input.FilteredPalmPos).AddQuaternion(Input.FilteredPalmRot)
                .AddVector3(Input.FilteredForward).AddVector3(Input.FilteredPointingPos).AddVector3(Input.FilteredPointingForward)
                .AddVector3(Input.m_up).AddVector3(Input.m_right).AddVector3(Input.m_forward)
                .AddVector3(Input.FilteredUp).AddVector3(Input.FilteredRight)
                .AddVector3(Input.VelLinearLocal).AddVector3(Input.VelAngularLocal).AddVector3(Input.VelLinearWorld).AddVector3(Input.VelAngularWorld)
                .Add(Input.IsGrabUp).Add(Input.IsGrabDown).Add(Input.IsGrabbing);
        }

        public void Deserialize(Message msg)
        {
            Input = new HandInput {
                TriggerUp = msg.GetBool(), TriggerDown = msg.GetBool(), TriggerPressed = msg.GetBool(), TriggerFloat = msg.GetFloat(),

                TriggerTouchUp = msg.GetBool(), TriggerTouchDown = msg.GetBool(), TriggerTouched = msg.GetBool(),

                GripUp = msg.GetBool(), GripDown = msg.GetBool(), GripPressed = msg.GetBool(),
                GripTouchUp = msg.GetBool(), GripTouchDown = msg.GetBool(), GripTouched = msg.GetBool(),

                TouchpadUp = msg.GetBool(), TouchpadDown = msg.GetBool(), TouchpadTouched = msg.GetBool(), TouchpadAxes = msg.GetVector3(),

                TouchpadNorthUp = msg.GetBool(), TouchpadNorthDown = msg.GetBool(), TouchpadNorthPressed = msg.GetBool(),
                TouchpadSouthUp = msg.GetBool(), TouchpadSouthDown = msg.GetBool(), TouchpadSouthPressed = msg.GetBool(),
                TouchpadWestUp = msg.GetBool(), TouchpadWestDown = msg.GetBool(), TouchpadWestPressed = msg.GetBool(),
                TouchpadEastUp = msg.GetBool(), TouchpadEastDown = msg.GetBool(), TouchpadEastPressed = msg.GetBool(),
                TouchpadCenterUp = msg.GetBool(), TouchpadCenterDown = msg.GetBool(), TouchpadCenterPressed = msg.GetBool(),

                BYButtonUp = msg.GetBool(), BYButtonDown = msg.GetBool(), BYButtonPressed = msg.GetBool(),
                AXButtonUp = msg.GetBool(), AXButtonDown = msg.GetBool(), AXButtonPressed = msg.GetBool(),

                Secondary2AxisInputUp = msg.GetBool(), Secondary2AxisInputDown = msg.GetBool(), Secondary2AxisInputPressed = msg.GetBool(),
                Secondary2AxisInputTouchUp = msg.GetBool(), Secondary2AxisInputTouchDown = msg.GetBool(), Secondary2AxisInputTouched = msg.GetBool(), Secondary2AxisInputAxes = msg.GetVector3(),
                Secondary2AxisNorthUp = msg.GetBool(), Secondary2AxisNorthDown = msg.GetBool(), Secondary2AxisNorthPressed = msg.GetBool(),
                Secondary2AxisSouthUp = msg.GetBool(), Secondary2AxisSouthDown = msg.GetBool(), Secondary2AxisSouthPressed = msg.GetBool(),
                Secondary2AxisWestUp = msg.GetBool(), Secondary2AxisWestDown = msg.GetBool(), Secondary2AxisWestPressed = msg.GetBool(),
                Secondary2AxisEastUp = msg.GetBool(), Secondary2AxisEastDown = msg.GetBool(), Secondary2AxisEastPressed = msg.GetBool(),
                Secondary2AxisCenterUp = msg.GetBool(), Secondary2AxisCenterDown = msg.GetBool(), Secondary2AxisCenterPressed = msg.GetBool(),

                FingerCurl_Thumb = msg.GetFloat(), FingerCurl_Index = msg.GetFloat(), FingerCurl_Middle = msg.GetFloat(), FingerCurl_Ring = msg.GetFloat(), FingerCurl_Pinky = msg.GetFloat(), LastCurlAverage = msg.GetFloat(),

                m_pos = msg.GetVector3(), m_rot = msg.GetQuaternion(), m_palmpos = msg.GetVector3(), m_palmrot = msg.GetQuaternion(),
                FilteredPos = msg.GetVector3(), FilteredRot = msg.GetQuaternion(), FilteredPalmPos = msg.GetVector3(), FilteredPalmRot = msg.GetQuaternion(),

                FilteredForward = msg.GetVector3(), FilteredPointingPos = msg.GetVector3(), FilteredPointingForward = msg.GetVector3(),

                m_up = msg.GetVector3(), m_right = msg.GetVector3(), m_forward = msg.GetVector3(),

                FilteredUp = msg.GetVector3(), FilteredRight = msg.GetVector3(),

                VelLinearLocal = msg.GetVector3(), VelAngularLocal = msg.GetVector3(), VelLinearWorld = msg.GetVector3(), VelAngularWorld = msg.GetVector3(),

                IsGrabUp = msg.GetBool(), IsGrabDown = msg.GetBool(), IsGrabbing = msg.GetBool(),
            };
        }
    }
}
