using System;
using System.Collections;
using System.Collections.Generic;
using H3MP.Server;
using UnityEngine;


namespace H3MP
{
	public class NetworkingPanelSwitcher : MonoBehaviour
	{
		public GameObject ConnectToServerPanel;
		public PlayersListPanel PlayersListPanel;

		private void Awake()
		{
			NetworkingPanel.ServerStarted += ShowPlayersList;
			NetworkingPanel.ConnectedToServer += ShowPlayersList;
		}

		private void OnEnable()
		{
			if (Plugin.Instance.Server.IsRunning)
				ShowPlayersList();
			else
				ShowConnectToServer();
		}

		public void ShowPlayersList()
		{
			ConnectToServerPanel.SetActive(false);
			PlayersListPanel.gameObject.SetActive(true);
		}

		public void ShowConnectToServer()
		{
			ConnectToServerPanel.SetActive(true);
			PlayersListPanel.gameObject.SetActive(false);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.A))
				ShowPlayersList();
			if (Input.GetKeyDown(KeyCode.B))
				ShowConnectToServer();
		}

		private void OnDestroy()
		{
			NetworkingPanel.ServerStarted -= ShowPlayersList;
			NetworkingPanel.ConnectedToServer -= ShowPlayersList;
		}
	}
}
