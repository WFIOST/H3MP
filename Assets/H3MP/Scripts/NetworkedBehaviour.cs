using System;
using H3MP.Core;
using UnityEngine;

namespace H3MP.Scripts
{
    public class NetworkedBehaviour : MonoBehaviour
    {
        public NetworkedObject Data;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb == null) rb = GetComponentInChildren<Rigidbody>();
            if (rb == null)
            {
                Destroy(this);
                throw new Exception("No rigidbody found on object " + gameObject.name);
            }
            
            _rigidbody = rb;
            
            SetData();
            
            NetworkManager.instance.RegisterObject(this);
        }

        private void FixedUpdate()
        {
            
        }

        private void SetData()
        {
            Data.Transform.Position = transform.position;
            Data.Transform.Rotation = transform.rotation;
            Data.Velocity = _rigidbody.velocity;
        }
        
        public void SetID(int id)
        {
            Data.ID = id;
        }

        public void Die()
        {
            
        }
    }
}