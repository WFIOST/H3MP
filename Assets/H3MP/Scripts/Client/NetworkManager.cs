using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Riptide;
using UnityEngine;

namespace H3MP.Scripts
{
    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager s_instance;
        public static NetworkManager Instance
        {
            get
            {
                if (s_instance == null) throw new NullReferenceException("Premature access to the network manager");
                return s_instance;
            }

            set
            {
                if (s_instance != null) throw new Exception("Instance already exists");
                s_instance = value;
            }
        }

        public Client Connection { get; private set; }
        
        

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
