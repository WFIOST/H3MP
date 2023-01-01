using System;
using Riptide;
using UnityEngine;

namespace H3MP.Server
{

    [Serializable]
    public struct SerialisableTransform : IMessageSerializable
    {
        public Vector3 Position, Scale;
        public Quaternion Rotation;


        public SerialisableTransform(Transform t)
        {
            Position = t.localPosition;
            Rotation = t.rotation;
            Scale = t.localScale;
        }

        public void Serialize(Message message)
        {
            message.AddVector3(Position).AddQuaternion(Rotation).AddVector3(Scale);
        }

        public void Deserialize(Message message)
        {
            Position = message.GetVector3();
            Rotation = message.GetQuaternion();
            Scale = message.GetVector3();
        }
    }
}
