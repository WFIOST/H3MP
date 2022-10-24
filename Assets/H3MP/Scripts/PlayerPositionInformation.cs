using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Riptide;
using FistVR;
using H3MP;
public class PlayerPositionInformation : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
	
	private void FixedUpdate()
    {
		SendPlayerPositionMessage();

	}
	public void SendPlayerPositionMessage()
    {
		Message message = Message.Create(MessageSendMode.Unreliable, messageType.PlayerPosition);
		NetVector3 pos = new NetVector3(GM.CurrentPlayerBody.transform.position);
		pos.Serialize(message);
		H3MP.H3MP.Instance.Send(message);
	}
    [MessageHandler(((ushort)messageType.PlayerPosition))]
	private static void ReceivePlayerPositionMessage(Message message)
    {
		NetVector3 PlayerPos = new NetVector3();
		PlayerPos.Deserialize(message);
		NetworkingPanel.NP.positionMover(PlayerPos);
		
	}
}
	public enum messageType
    {
	PlayerPosition,
    }
public struct NetVector3 : IMessageSerializable
{
	public NetVector3 (Vector3 vec)
    {
		x = vec.x;
		y = vec.y;
		z = vec.z;
    }
	public float x, y, z;
	public void Serialize(Message msg)
	{
		msg.Add(x).Add(y).Add(z);
	}


	public void Deserialize(Message message)
	{
		x = message.GetFloat();
		y = message.GetFloat();
		z = message.GetFloat();
	}
	
}
