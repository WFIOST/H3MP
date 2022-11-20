using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using H3MP.Core;
using Riptide;

public class PhantomHand : FVRViveHand {
    public Vector3 LastPos;
    public Quaternion LastRotation;

    private SerialisableInput _input;
    
    //setting this as new might mean it isn't properly called
    public void Start()
    {
        base.Start();
        m_initState = HandInitializationState.Uninitialized;
        IsInDemoMode = true;
        Input = new HandInput();
        Buzzer = gameObject.AddComponent<FVRHaptics>();
        
    }

    public void InputUpdate(SerialisableInput input)
    {
        Input = new HandInput
        {
            
        };
    }
}