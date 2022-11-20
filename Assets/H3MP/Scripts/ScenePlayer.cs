using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using H3MP.Core;
using FistVR;
public class ScenePlayer : MonoBehaviour {

	public GameObject Head;
	public GameObject LeftHand;
	public Vector3 LeftHandLastPos = Vector3.zero;
	public Quaternion LeftHandLastAngularPos =  Quaternion.identity;
	public GameObject RightHand;
	public Vector3 RightHandLastPos = Vector3.zero;
	public Quaternion RightHandLastAngularPos = Quaternion.identity;
	public string Name;
	public int ScoreBoardPosition;
	public Text namePlate;
	public ushort ID;
	public GameObject SosigHead, SosigTorso;
	public PhantomHand LeftFakeHand, RightFakeHand;

	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		SosigHead.transform.SetPositionAndRotation(SosigHead.transform.position, Head.transform.rotation);
		SosigTorso.transform.position.Set(Head.transform.position.x, Head.transform.position.y - 1, Head.transform.position.z);
	}
	public void UpdatePlayer(Player PS)
    {
		LeftHandLastPos = LeftHand.transform.position;
		LeftHandLastAngularPos = LeftHand.transform.rotation;
		RightHandLastPos = RightHand.transform.position;
		RightHandLastAngularPos = RightHand.transform.rotation;
	    Head.transform.SetPositionAndRotation(PS.Head.position, PS.Head.rotation);
		LeftHand.transform.SetPositionAndRotation(PS.LeftHand.Transform.position, PS.LeftHand.Transform.rotation);
		RightHand.transform.SetPositionAndRotation(PS.RightHand.Transform.position, PS.RightHand.Transform.rotation);
		
	}


	public void InputUpdate(SerialisableInput input)
	{
		if (input.IsRightHand) RightFakeHand.InputUpdate(input);
		else LeftFakeHand.InputUpdate(input);
	}
}
