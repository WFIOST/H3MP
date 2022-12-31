using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;


namespace H3MP
{
	public class NetworkingPanelButton : MonoBehaviour
	{
		public Text textbox;
		private Text thisString;
		public buttonTypes buttonType;

		// Use this for initialization
		void Start()
		{
			thisString = GetComponentInChildren<Text>();
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void AddText()
		{
			if (buttonType == buttonTypes.Input)
			{
				textbox.text += thisString.text;
			}
			else if (buttonType == buttonTypes.Clear)
			{
				textbox.text = null;
			}
			else if (buttonType == buttonTypes.Enter)
			{
				NetworkingPanel.NP.IPaddress = textbox.text;
				NetworkingPanel.NP.Connect();
			}
			else if (buttonType == buttonTypes.backspace)
			{
				textbox.text = textbox.text.Remove(textbox.text.Length - 1, 1);
			}
			else if (buttonType == buttonTypes.StartServer)
			{
				string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
				var externalIp = IPAddress.Parse(externalIpString);

				textbox.text = externalIp.ToString();
				NetworkingPanel.NP.CreateServer();
			}
		}

		public enum buttonTypes
		{
			Clear,
			Enter,
			Input,
			backspace,
			StartServer
		}
	}

}
