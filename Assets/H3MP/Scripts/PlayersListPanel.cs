using System;
using System.Collections;
using System.Collections.Generic;
using H3MP.Core;
using UnityEngine;
using UnityEngine.UI;

public class PlayersListPanel : MonoBehaviour
{
	public List<PlayerTab> PlayerTabs;
	public PlayerTab PlayerTabPrefab;
	public Transform PlayersListParent;

	public Text StopServerText;
	private bool _stopServerConfirm;
	
	public Text LobbyCountText;

	private void Awake()
	{
		NetworkingPanel.ServerStarted += RefreshUI;
		NetworkManager.PlayerConnectedEvent += RefreshUI;
		NetworkingPanel.ConnectedToServer += RefreshUI;
	}

	private void Start()
	{
		// Remove preview tabs
		for (int i = PlayerTabs.Count - 1; i >= 0; i--)
		{
			RemovePlayerTab(PlayerTabs[i]);
		}
	}

	public void RefreshUI()
	{
		for (int i = PlayerTabs.Count - 1; i >= 0; i--)
		{
			RemovePlayerTab(PlayerTabs[i]);
		}
		
		foreach (var player in NetworkManager.instance.scenePlayers)
		{
			AddPlayerTab(player);
		}
	}

	public void AddPlayerTab(ScenePlayer player)
	{
		PlayerTab newTab = Instantiate(PlayerTabPrefab, PlayersListParent);
		newTab.Initialize(this, player);
		PlayerTabs.Add(newTab);
		UpdateLobbyCount();
	}

	//TODO This method should be called only on RemovePlayer in NetworkingManager or smth, not manually when clicked here
	public void RemovePlayerTab(PlayerTab tab)
	{
		PlayerTabs.Remove(tab);
		Destroy(tab.gameObject);
		UpdateLobbyCount();
	}

	public void StopServerOrDisconnect()
	{
		if (Plugin.Instance.IsServer)
		{
			if (_stopServerConfirm)
			{
				//TODO Stop server
				ResetConfirms();
			}
			else
			{
				_stopServerConfirm = true;
				StopServerText.text = "Confirm";
			}
		}
		else
		{
			if (_stopServerConfirm)
			{
				//TODO Disconnect the player
				ResetConfirms();
			}
			else
			{
				_stopServerConfirm = true;
				StopServerText.text = "Confirm";
			}
		}
	}
	
	public void ResetConfirms()
	{
		foreach (var tab in PlayerTabs)
		{
			tab.ResetConfirm();
		}
		
		if (Plugin.Instance.IsServer)
			StopServerText.text = "Stop Server";
		else
			StopServerText.text = "Disconnect";
	}
	
	private void UpdateLobbyCount()
	{
		LobbyCountText.text = NetworkManager.instance.scenePlayers.Count + "/" + "??" + " Players";
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
			RefreshUI();
	}

	private void OnDestroy()
	{
		NetworkingPanel.ServerStarted -= RefreshUI;
		NetworkManager.PlayerConnectedEvent -= RefreshUI;
		NetworkingPanel.ConnectedToServer -= RefreshUI;
	}
}
