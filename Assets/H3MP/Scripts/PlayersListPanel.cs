using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersListPanel : MonoBehaviour
{
	public List<PlayerTab> PlayerTabs;
	public PlayerTab PlayerTabPrefab;
	public Transform PlayersListParent;

	public static int PlayerCount = 4;

	public Text StopServerText;
	private bool _stopServerConfirm;
	
	public Text LobbyCountText;
	
	private void Start()
	{
		// Remove preview tabs
		for (int i = PlayerTabs.Count - 1; i >= 0; i--)
		{
			RemovePlayerTab(PlayerTabs[i]);
		}
		
		// Simulate players joining
		for (int i = 0; i < PlayerCount; i++)
		{
			AddPlayerTab();
		}
	}

	public void AddPlayerTab()
	{
		PlayerTab newTab = Instantiate(PlayerTabPrefab, PlayersListParent);
		newTab.Initialize(this);
		PlayerTabs.Add(newTab);
		UpdateLobbyCount();
	}

	public void RemovePlayerTab(PlayerTab tab)
	{
		PlayerTabs.Remove(tab);
		Destroy(tab.gameObject);
		UpdateLobbyCount();
	}

	public void StopServerClicked()
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
	
	public void ResetConfirms()
	{
		foreach (var tab in PlayerTabs)
		{
			tab.ResetConfirm();
		}
		
		StopServerText.text = "Stop Server";
	}
	
	private void UpdateLobbyCount()
	{
		LobbyCountText.text = PlayerTabs.Count + "/" + PlayerCount + " Players";
	}
}
