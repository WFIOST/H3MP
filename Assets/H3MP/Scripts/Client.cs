using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Riptide;
using UnityEngine;

namespace H3MP.Scripts
{
    public class Client : MonoBehaviour
    {
        private Riptide.Client _connection;
        
        private void Awake()
        {
            _connection = new Riptide.Client();
        }

        public void ConnectTo(string addr)
        {
            _connection.Connect(hostAddress: addr);
        }
    }
}
