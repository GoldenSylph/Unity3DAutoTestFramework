﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATF
{
    public class ATFStandaloneInputManager : StandaloneInputModule
    {

        /*
         * emit the input
         * struct for test scenario
         * 
         * */

        /*
         *Interceptors for Input and All events
         *
         * */

        protected override void Awake()
        {
            base.Awake();
            m_InputOverride = gameObject.AddComponent<ATFInput>();
        }
    }
}
