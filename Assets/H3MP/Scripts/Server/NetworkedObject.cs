using System;
using Riptide;
using UnityEngine;

namespace H3MP.Server;

[Serializable]
public class NetworkedObject : IMessageSerializable, IEquatable<NetworkedObject>
{
	public int						ID;
	public SerialisableTransform	Transform;
	public Vector3					Velocity;
	public string					ObjectID;


	public NetworkedObject(int id)
	{
		ID = id;
		Transform = new SerialisableTransform();
		Velocity = new Vector3();
		ObjectID = "";
	}

	public NetworkedObject() : this(0){}

	public void Serialize(Message message) => message.Add(ID).Add(ObjectID).Add(Transform).AddVector3(Velocity);
	
	public void Deserialize(Message message)
	{
		ID = message.GetInt();
		ObjectID = message.GetString();
		Transform.Deserialize(message);
		Velocity = message.GetVector3();
	}

	public override bool Equals(object? obj) =>
		ReferenceEquals(objA: this, objB: obj) || obj is NetworkedObject other && Equals(other);

	public override int GetHashCode() => ID;
	public bool Equals(NetworkedObject? other)
	{
		if (ReferenceEquals(objA: null, objB: other)) return false;
		if (ReferenceEquals(objA: this, objB: other)) return true;
		return ID == other.ID;
	}

	public static bool operator ==(NetworkedObject? left, NetworkedObject? right) => Equals(objA: left, objB: right);
	public static bool operator !=(NetworkedObject? left, NetworkedObject? right) => !Equals(objA: left, objB: right);
}
