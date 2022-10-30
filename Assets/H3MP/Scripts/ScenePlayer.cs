using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using H3MP.Core;
public class ScenePlayer : MonoBehaviour {

	public GameObject Head;
	public GameObject LeftHand;
	public GameObject RightHand;
	public string Name;
	public int ScoreBoardPosition;
	public Text namePlate;
	public ushort ID;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void UpdatePlayer(Player PS)
    {
	    Head.transform.SetPositionAndRotation(PS.Head.Position, PS.Head.Rotation);		
		LeftHand.transform.SetPositionAndRotation(PS.LeftHand.Position, PS.LeftHand.Rotation);
		RightHand.transform.SetPositionAndRotation(PS.RightHand.Position, PS.RightHand.Rotation);
		
	}
}
