using System;
using FistVR;
using Riptide;
using UnityEngine;

namespace H3MP.Server
{

    /// <summary>
    /// Player object to handle sync, serialization, and sending information to and from the server.
    /// </summary>
    [Serializable]
    public class Player : IEquatable<Player>, IMessageSerializable
    {
        public SerialisableTransform Head;
        public Hand LeftHand, RightHand;

        public ushort ID;
        public string Username;


        public Player()
        {
            Head = new SerialisableTransform();
            LeftHand = new Hand();
            RightHand = new Hand();

            Username = "";
            ID = 0;
        }

        public Player(ushort userID, string username, Transform head, FVRViveHand lhand, FVRViveHand rhand)
        {
            ID = userID;
            Username = username;

            Head = new SerialisableTransform(head);
            LeftHand = new Hand(lhand);
            RightHand = new Hand(rhand);
        }

        public bool Equals(Player other)
        {
            return ID == other.ID;
        }

        public void Serialize(Message message)
        {
            message.Add(ID).Add(Username).Add(Head).Add(LeftHand).Add(RightHand);
        }

        public void Deserialize(Message message)
        {
            ID = message.GetUShort();
            Username = message.GetString();
            Head.Deserialize(message);
            LeftHand.Deserialize(message);
            RightHand.Deserialize(message);
        }
    }
}
