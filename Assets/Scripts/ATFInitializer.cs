using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.DI;
using UnityEditor;

namespace ATF
{
    [Injectable]
    public class ATFInitializer : MonoBehaviour
    {
        private void Awake()
        {
            DependencyInjector.Instance.Initialize("ATF");
            DependencyInjector.Instance.Inject();
        }
    }
}