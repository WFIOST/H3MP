using System;
using System.Collections;
using System.Collections.Generic;
using H3MP.Core;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTab : MonoBehaviour 
{
	public Text NameText;
	public Text PingText;
	
	public Button KickButton;
	public Button BanButton;
	
	public Text KickText;
	public Text BanText;

	private bool _confirmKick;
	private bool _confirmBan;

	private PlayersListPanel _panel;
	private ScenePlayer _player;
	
	public void Initialize(PlayersListPanel panel, ScenePlayer player)
	{
		_panel = panel;
		_player = player;

		NameText.text = player.Name;
		PingText.text = "???ms";

		if (!Plugin.Instance.IsServer || player.ID == Plugin.Instance.Client.Id)
		{
			KickButton.gameObject.SetActive(false);
			BanButton.gameObject.SetActive(false);
		}
	}
	
	public void KickClicked()
	{
		if (_confirmKick)
		{
			//TODO Kick
			_panel.RemovePlayerTab(this);
			_panel.ResetConfirms();
		}
		else
		{
			_confirmKick = true;
			KickText.text = "Confirm";
		}
	}

	public void BanClicked()
	{
		if (_confirmBan)
		{
			//TODO Ban
			_panel.RemovePlayerTab(this);
			_panel.ResetConfirms();
		}
		else
		{
			_confirmBan = true;
			BanText.text = "Confirm";
		}
	}

	public void ResetConfirm()
	{
		_confirmKick = false;
		_confirmBan = false;
		
		KickText.text = "Kick";
		BanText.text = "Ban";
	}
}
