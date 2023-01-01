local input = [[
// Decompiled with JetBrains decompiler
// Type: FistVR.HandInput
// Assembly: Assembly-CSharp, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AB4DB40E-8FBD-4133-A4B9-C19BB6F4ED59
// Assembly location: /Users/frityet/.nuget/packages/h3vr.gamelibs/0.105.4/lib/net35/Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FistVR
{
    [Serializable]
    public struct HandInput
    {
        public FVRViveHand Hand;
        public bool TriggerUp;
        public bool TriggerDown;
        public bool TriggerPressed;
        public float TriggerFloat;
        public bool TriggerTouchUp;
        public bool TriggerTouchDown;
        public bool TriggerTouched;
        public bool GripUp;
        public bool GripDown;
        public bool GripPressed;
        public bool GripTouchUp;
        public bool GripTouchDown;
        public bool GripTouched;
        public bool TouchpadUp;
        public bool TouchpadDown;
        public bool TouchpadPressed;
        public bool TouchpadTouchUp;
        public bool TouchpadTouchDown;
        public bool TouchpadTouched;
        public Vector2 TouchpadAxes;
        public bool TouchpadNorthUp;
        public bool TouchpadNorthDown;
        public bool TouchpadNorthPressed;
        public bool TouchpadSouthUp;
        public bool TouchpadSouthDown;
        public bool TouchpadSouthPressed;
        public bool TouchpadWestUp;
        public bool TouchpadWestDown;
        public bool TouchpadWestPressed;
        public bool TouchpadEastUp;
        public bool TouchpadEastDown;
        public bool TouchpadEastPressed;
        public bool TouchpadCenterUp;
        public bool TouchpadCenterDown;
        public bool TouchpadCenterPressed;
        public bool BYButtonUp;
        public bool BYButtonDown;
        public bool BYButtonPressed;
        public bool AXButtonUp;
        public bool AXButtonDown;
        public bool AXButtonPressed;
        public bool Secondary2AxisInputUp;
        public bool Secondary2AxisInputDown;
        public bool Secondary2AxisInputPressed;
        public bool Secondary2AxisInputTouchUp;
        public bool Secondary2AxisInputTouchDown;
        public bool Secondary2AxisInputTouched;
        public Vector2 Secondary2AxisInputAxes;
        public bool Secondary2AxisNorthUp;
        public bool Secondary2AxisNorthDown;
        public bool Secondary2AxisNorthPressed;
        public bool Secondary2AxisSouthUp;
        public bool Secondary2AxisSouthDown;
        public bool Secondary2AxisSouthPressed;
        public bool Secondary2AxisWestUp;
        public bool Secondary2AxisWestDown;
        public bool Secondary2AxisWestPressed;
        public bool Secondary2AxisEastUp;
        public bool Secondary2AxisEastDown;
        public bool Secondary2AxisEastPressed;
        public bool Secondary2AxisCenterUp;
        public bool Secondary2AxisCenterDown;
        public bool Secondary2AxisCenterPressed;
        public float FingerCurl_Thumb;
        public float FingerCurl_Index;
        public float FingerCurl_Middle;
        public float FingerCurl_Ring;
        public float FingerCurl_Pinky;
        public float LastCurlAverage;
        [Publicized(1)]
        public Vector3 m_pos;
        [Publicized(1)]
        public Quaternion m_rot;
        [Publicized(1)]
        public Vector3 m_palmpos;
        [Publicized(1)]
        public Quaternion m_palmrot;
        public Vector3 FilteredPos;
        public Quaternion FilteredRot;
        public Vector3 FilteredPalmPos;
        public Quaternion FilteredPalmRot;
        public Vector3 FilteredForward;
        public Vector3 FilteredPointingPos;
        public Vector3 FilteredPointingForward;
        [Publicized(1)]
        public Vector3 m_up;
        [Publicized(1)]
        public Vector3 m_right;
        [Publicized(1)]
        public Vector3 m_forward;
        public Vector3 FilteredUp;
        public Vector3 FilteredRight;
        public Vector3 VelLinearLocal;
        public Vector3 VelAngularLocal;
        public Vector3 VelLinearWorld;
        public Vector3 VelAngularWorld;
        public bool IsGrabUp;
        public bool IsGrabDown;
        public bool IsGrabbing;
        [Publicized(1)]
        public OneEuroFilter<Vector3> positionFilter;
        [Publicized(1)]
        public OneEuroFilter<Quaternion> rotationFilter;
        [Publicized(1)]
        public OneEuroFilter<Vector3> positionFilterPalm;
        [Publicized(1)]
        public OneEuroFilter<Quaternion> rotationFilterPalm;
        [Publicized(1)]
        public OneEuroFilter<Vector3> pointingPosFilter;
        [Publicized(1)]
        public OneEuroFilter<Quaternion> pointingRotFilter;
        [Publicized(1)]
        public OneEuroFilter<Vector3> positionUltraFilter;
        [Publicized(1)]
        public OneEuroFilter<Quaternion> rotationUltraFilter;
        public Vector3 PosUltraFilter;
        public Quaternion RotUltraFilter;
        [Publicized(1)]
        public Vector3 m_oneEuroLocalPosition;
        [Publicized(1)]
        public Vector3 m_oneEuroLocalPalmPosition;
        [Publicized(1)]
        public Vector3 m_oneEuroLocalPointingPosition;
        public Quaternion OneEuroRotation;
        public Quaternion OneEuroPalmRotation;
        public Quaternion OneEuroPointRotation;
        public Vector3 LastPalmPos1;
        public Vector3 LastPalmPos2;
        public Transform EuroTester;

        public Vector3 Pos
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
            [MethodImpl(MethodImplOptions.NoInlining)] set => throw null;
        }

        public Quaternion Rot
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
            [MethodImpl(MethodImplOptions.NoInlining)] set => throw null;
        }

        public Vector3 PalmPos
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
            [MethodImpl(MethodImplOptions.NoInlining)] set => throw null;
        }

        public Quaternion PalmRot
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
            [MethodImpl(MethodImplOptions.NoInlining)] set => throw null;
        }

        public Vector3 Up
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        public Vector3 Right
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        public Vector3 Forward
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        public OneEuroFilter<Vector3> PUF
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        public OneEuroFilter<Quaternion> RUF
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        public Vector3 OneEuroPosition
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        public Vector3 OneEuroPalmPosition
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        public Vector3 OneEuroPointingPos
        {
            [MethodImpl(MethodImplOptions.NoInlining)] get => throw null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Init() => throw null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Flush() => throw null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void UpdateEuroFilter() => throw null;
    }
}

]]

local excludenames = {
    "HandInput",
    "Hand",
    "Up",
    "Right",
    "Forward",
    "PUF",
    "RUF",
    "OneEuroPosition",
    "OneEuroPalmPosition",
    "OneEuroPointingPos",
}
local excludetypes = {
    "OneEuroFilter",
    "Transform"
}

local type_deserialisation_functions = {
    ["bool"] = "GetBool",
    ["Vector2"] = "GetVector2",
    ["Vector3"] = "GetVector3",
    ["Quaternion"] = "GetQuaternion",
    ["float"] = "GetFloat",
}

---@type { type: string, name: string }[]
local input_names = { type = "", name = "" }
for type, name in input:gmatch("public ([^%s]+) ([^%s;]+)") do
    --Literally just to force annotations
    ---@type string
    name = name

    for _, v in ipairs(excludenames) do if name == v then goto next end end
    for _, v in ipairs(excludetypes) do if type:find(v) then goto next end end
    if name:find("%b()") then goto next end

    table.insert(input_names, { type = type, name = name })

    ::next::
end

local f = io.open(arg[1] or "InputIDs.cs", "w+")
if not f then error("Could not open file for writing!") end

f:write [[/*
This file was generated by generate-input-identifiers.lua
Please do not directly modify this cs file!
To regenerate, go to root dir of this project and run
"lua scripts/generate-input-identifiers.lua H3MP/Core/InputIdentifiers.cs"
*/

namespace H3MP.Core;

public enum InputIdentifier : byte
{
]]

for i, v in ipairs(input_names) do
    f:write(string.format("    %s = %d,\n", v.name, i))
end

f:write("}\n")

f:close()

io.write("Generate switch statement for setting input? [Y/N]: ")
local ok do
    ---@type string
    local tmp = io.read("l")
    ok = tmp:sub(1, 1):lower() == "y" or #tmp == 0
end
if not ok then return end

io.write("return id switch\n")
io.write("{")
for _, v in ipairs(input_names) do
    io.write(string.format("    %s => Input.%s = msg.%s(),\n", "InputIdentifier."..v.name, v.name, type_deserialisation_functions[v.type]))
end
io.write("    _ => throw new ArgumentOutOfRangeException()\n")
io.write("};\n")
