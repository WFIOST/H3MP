using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using H3MP.Core;
using FistVR;
public class ScenePlayer : MonoBehaviour {

	public GameObject Head;
	public GameObject LeftHand, RightHand;
	public Vector3 LeftHandLastPos = Vector3.zero;
	public Quaternion LeftHandLastAngularPos =  Quaternion.identity;
	public Vector3 RightHandLastPos = Vector3.zero;
	public Quaternion RightHandLastAngularPos = Quaternion.identity;
	public string Name;
	public int ScoreBoardPosition;
	public Text namePlate;
	public ushort ID;
	public GameObject SosigHead, SosigTorso;

	[SerializeField]
	private FVRViveHand _leftFakeHand, _rightFakeHand;

	void Start ()
	{
		_leftFakeHand = Instantiate(GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>(), LeftHand.transform);
		_rightFakeHand = Instantiate(GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>(), RightHand.transform);

		_leftFakeHand.IsInDemoMode = true;
		_rightFakeHand.IsInDemoMode = true;
	}
	
	void Update () 
	{
		SosigHead.transform.SetPositionAndRotation(SosigHead.transform.position, Head.transform.rotation);
		SosigTorso.transform.position.Set(Head.transform.position.x, Head.transform.position.y - 1, Head.transform.position.z);
	}
	public void UpdatePlayer(Player PS)
    {
		LeftHandLastPos = LeftHand.transform.position;
		LeftHandLastAngularPos = LeftHand.transform.rotation;
		RightHandLastPos = RightHand.transform.position;
		RightHandLastAngularPos = RightHand.transform.rotation;
	    Head.transform.SetPositionAndRotation(PS.Head.Position, PS.Head.Rotation);
		LeftHand.transform.SetPositionAndRotation(PS.LeftHand.Transform.Position, PS.LeftHand.Transform.Rotation);
		RightHand.transform.SetPositionAndRotation(PS.RightHand.Transform.Position, PS.RightHand.Transform.Rotation);
		
	}


	public void InputUpdate(SerialisableInput input)
	{
		if (input.IsRightHand) _rightFakeHand.Input = input.Input;
		else _leftFakeHand.Input = input.Input;
	}
}
