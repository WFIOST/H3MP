using System;
using FistVR;
using Riptide;
using UnityEngine;

namespace H3MP.Server;

[Serializable]
public struct Hand : IMessageSerializable
{
    public SerialisableTransform    Transform;
    public NetworkedObject?         Holding;
    public SerialisableInput        Input;

    public Hand(FVRViveHand reference)
    {
        Transform = new SerialisableTransform(reference.transform);
        Input = new SerialisableInput(reference);
        Holding = null;
    }
    
    public void Serialize(Message message)
    {
        message.Add(Transform).Add(Input);

        if (Holding is not null) message.Add(Holding);
    }

    public void Deserialize(Message message)
    {
        Transform = message.GetSerializable<SerialisableTransform>();
        Input = message.GetSerializable<SerialisableInput>();

        try { Holding = message.GetSerializable<NetworkedObject>(); }
        catch { Holding = null; }
    }
}