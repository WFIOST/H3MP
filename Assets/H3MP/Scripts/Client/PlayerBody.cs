using System;
using FistVR;
using H3MP.Server;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace H3MP.Scripts
{
    public class PlayerBody : MonoBehaviour
    { 
        public Player PlayerInfo { get; private set; }
        public GameObject Head, Torso;
        public FVRViveHand LeftHand, RightHand;
        public Text UsernamePlate;

        private void Awake()
        {
            PlayerInfo = new Player();
            
            LeftHand = Instantiate(GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>(), LeftHand.transform);
            RightHand = Instantiate(GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>(), RightHand.transform);

            LeftHand.IsInDemoMode = true;
            RightHand.IsInDemoMode = true;
        }

        private void FixedUpdate()
        {
            Head.transform.SetPositionAndRotation(PlayerInfo.Head.Position, PlayerInfo.Head.Rotation);
            Torso.transform.position = new Vector3(PlayerInfo.Head.Position.x, PlayerInfo.Head.Position.y - 1, PlayerInfo.Head.Position.z);

            LeftHand.Input  = PlayerInfo.LeftHand.Input.Input;
            RightHand.Input = PlayerInfo.RightHand.Input.Input;
        }
    }
}