using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTab : MonoBehaviour 
{
	public Text NameText;
	public Text PingText;
	
	public Text KickText;
	public Text BanText;

	private bool _confirmKick;
	private bool _confirmBan;

	private PlayersListPanel _panel;
	
	public void Initialize(PlayersListPanel panel)
	{
		// if (IsYou)
		// 	Destroy(gameObject);

		_panel = panel;

		NameText.text = "Player #1";
		PingText.text = "420ms";
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
