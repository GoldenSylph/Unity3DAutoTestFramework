using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Bedrin.DI;

namespace ATF
{
    public class ATFStandaloneInputManager : StandaloneInputModule
    {
        protected override void Start()
        {
            base.Start();
            m_InputOverride = gameObject.AddComponent<ATFInput>();
            DependencyInjector.Instance.InjectType(m_InputOverride.GetType());
        }
    }
}
