using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using Sodalite.Api;
using Sodalite.ModPanel;
using System.IO;
using System.Reflection;
using FistVR;
using H3MP.Core;
//using H3MP.Common;
using BepInEx;
[BepInPlugin("h3vr.arpy.H3MP", "NetworkingPanel", "1.0.0")]
public class NetworkingPanel : BaseUnityPlugin {
	//everything within these comments is temporary and should be moved to a better place once you figure out where that would be
	public GameObject PlayerPositionReference;
	//everything within these comments is temporary and should be moved to a better place once you figure out where that would be
	public static NetworkingPanel NP;
	public string IPaddress = "00000000";
	private GameObject _modPanelPrefab;
	private LockablePanel _modPanel = null;
	private UniversalModPanel _modPanelComponent;
	public GameObject panel;
	private static readonly string BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	public void Awake () {
		var bundle = AssetBundle.LoadFromFile(Path.Combine(BasePath, "H3MP"));
		if (bundle)
		{
				
			_modPanelPrefab = bundle.LoadAsset<GameObject>("NetworkingPanelUI");
			_modPanel = new LockablePanel();
			_modPanel.Configure += ConfigureModPanel;
			
		}
		NP = this;
	}
	void Start()
    {
		//everything within these comments is temporary and should be moved to a better place once you figure out where that would be
		PlayerPositionReference = Instantiate(PlayerPositionReference);
		PlayerPositionReference.transform.position = GM.CurrentPlayerBody.transform.position;
		//everything within these comments is temporary and should be moved to a better place once you figure out where that would be
	}
	// Update is called once per frame
	public void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha7))
        {
			Debug.Log("KeyDown");
			createPanel();
        }
	}
	public void Connect()
	{
		Plugin.Instance.ConnectTo(IPaddress);
		
	}
	public string ScreenName;
	private void SpawnModPanel() //This spawns the disclaimer panel in your left hand
	{
		panel = _modPanel.GetOrCreatePanel();
		GM.CurrentMovementManager.Hands[0].RetrieveObject(panel.GetComponent<FVRPhysicalObject>());
	}

	private void ConfigureModPanel(GameObject panel) // This configures the disclaimer panel with the proper UI
	{
		var canvasTransform = panel.transform.Find("OptionsCanvas_0_Main/Canvas");
		_modPanelComponent = Instantiate(_modPanelPrefab, canvasTransform.position, canvasTransform.rotation, canvasTransform.parent).GetComponent<UniversalModPanel>();
		Destroy(canvasTransform.gameObject);
	}
	public void createPanel()
    {
		Debug.Log("CreatePanel");
		LockablePanel BloodOathPanel = new LockablePanel();
		BloodOathPanel.Configure += ConfigureModPanel;
		SpawnModPanel();
	}
	public void CreateServer()
    {
		Plugin.Instance.StartServer();
		
		panel.transform.GetChild(0).gameObject.SetActive(false);// Disable Connect UI
		panel.transform.GetChild(1).gameObject.SetActive(true); // Enable Player list UI
    }
	//everything within these comments is temporary and should be moved to a better place once you figure out where that would be
	public void positionMover(NetVector3 pos)
    {
		PlayerPositionReference.transform.position = new Vector3(pos.x, pos.y, pos.z);
    }
	//everything within these comments is temporary and should be moved to a better place once you figure out where that would be
}
