using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ATFStandaloneInputManager : StandaloneInputModule
{
    protected override void Awake()
    {
        base.Awake();
        m_InputOverride = gameObject.AddComponent<ATFInput>();
    }
}
