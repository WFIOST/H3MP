using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using Riptide;
using H3MP.Core;
public class HandManager : MonoBehaviour {

	// Use this for initialization
	private FVRViveHand LeftHand;
	private FVRViveHand RightHand;
	private HandInput LeftStoredInputs;
	private HandInput RightStoredInputs;
	void Start() {
		LeftHand = GM.CurrentMovementManager.Hands[0];
		RightHand = GM.CurrentMovementManager.Hands[1];
		LeftStoredInputs = new HandInput();
		RightStoredInputs = new HandInput();
		
	}
	
	// Update is called once per frame
	void Update () {
        //Left Hand Checks
        #region Left Hand
        if (LeftHand.Input.GripPressed != LeftStoredInputs.GripPressed)
		{
			LeftStoredInputs.GripDown = LeftHand.Input.GripDown;
			SendInputMessage(InputUpdate.Inputs.GRIP_DOWN, true, -1);
		}
		if (LeftHand.Input.GripUp != LeftStoredInputs.GripUp)
        {
			LeftStoredInputs.GripUp = LeftHand.Input.GripUp;
			SendInputMessage(InputUpdate.Inputs.GRIP_UP, true, -1);
		}
		#endregion
		#region Right Hand
		if (RightHand.Input.GripPressed != RightStoredInputs.GripPressed)
		{
			RightStoredInputs.GripDown = RightHand.Input.GripDown;
			SendInputMessage(InputUpdate.Inputs.GRIP_DOWN, false, -1);
		}
		if (RightHand.Input.GripUp != RightStoredInputs.GripUp)
		{
			RightStoredInputs.GripUp = RightHand.Input.GripUp;
			SendInputMessage(InputUpdate.Inputs.GRIP_UP, false, -1);
		}
		#endregion

	}
	public void SendInputMessage(InputUpdate.Inputs InputEnum, bool isLeftHand, float triggerFloat)
    {
		//Update with proper Enum
		Message msg = Message.Create(MessageSendMode.Reliable, (ushort)0011);
		InputUpdate IU = new InputUpdate(isLeftHand, InputEnum, triggerFloat);
		msg.Add(Plugin.Instance.Client.Id);
		msg.Add(IU);
    }
	public void HandleInputMessage(Message msg)
    {
		ushort id = msg.GetUShort();
		InputUpdate IU = msg.GetSerializable<InputUpdate>();
		foreach (ScenePlayer player in NetworkManager.instance.scenePlayers)
        {
			if (player.ID == id)
            {
				player.ReceiveInputMessage(IU);
				break;
            }
        }


    }
}
