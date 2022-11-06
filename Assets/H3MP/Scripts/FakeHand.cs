using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
public class FakeHand : FVRViveHand {
    public Vector3 LastPos;
    public Quaternion LastRotation;
    //setting this as new might mean it isn't properly called
    public new void Start()
    {
        base.Start();
        m_initState = HandInitializationState.Uninitialized;
        IsInDemoMode = true;
        Input = new HandInput();
        Buzzer = gameObject.AddComponent<FVRHaptics>();
        
    }
    //setting this as new might mean it isn't properly called
    public new void Update()
    {
        base.Update();
        Vector3 velocity = (transform.position - LastPos) / Time.deltaTime;

        //This creates fluctuation when moving over max or min
        //Vector3 angularVelocity = (Quaternion.Inverse(prevRotation) * transform.rotation).eulerAngles / Time.deltaTime;

        //I found this bit of code from here: https://forum.unity.com/threads/manually-calculate-angular-velocity-of-gameobject.289462/
        var deltaRot = transform.rotation * Quaternion.Inverse(LastRotation);
        var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));

        Vector3 angularVelocity = eulerRot;

        //AnimLogger.Log("Velocity: " + velocity.ToString("F3") + ", Angular Velocity: " + angularVelocity.ToString("F3"));

        Input.VelLinearLocal = velocity;
        Input.VelLinearWorld = velocity;

        Input.VelAngularLocal = angularVelocity;
        Input.VelAngularWorld = angularVelocity;
        SendUpdate();
    }
    public void SendUpdate()
    {
		
    }
    
}
