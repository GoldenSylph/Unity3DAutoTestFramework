using System;
using System.Collections.Generic;
using System.Reflection;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Integration;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;
using UnityEngine;

namespace ATF.Scripts
{
    public class AtfInitializer : MonoSingleton<AtfInitializer>
    {
        [SerializeField]
        public bool isDebugPrintOn = true;
        
        public const string ATF_NAMESPACE_NAME = "ATF";

        private static void Print(object obj)
        {
            if (Instance.isDebugPrintOn)
            {
                print(obj);
            }
        }
        
        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            Debug.LogWarning("ATF is enabled. Creating systems...");
            var atfSystemsTypes =
                DependencyInjector.GetAttributeTypesInNamespace(ATF_NAMESPACE_NAME, typeof(AtfSystemAttribute));
            foreach (var systemType in atfSystemsTypes)
            {
                var systemInstance = systemType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?.GetValue(null, null) as IAtfInitializable;
                Print(systemInstance);
                systemInstance?.Initialize();
            }
            DependencyInjector.Instance.Inject();
            Debug.LogWarning("ATF is now ready to work. Please open the control windows.");
        }
    }
}