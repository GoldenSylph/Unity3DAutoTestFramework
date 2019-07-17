using System;
using ATF.Scripts.Integration;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;
using Bedrin.DI;
using Bedrin.Helper;
using UnityEngine;

namespace ATF.Scripts
{
    public class AtfInitializer : MonoSingleton<AtfInitializer>
    {
        [SerializeField]
        public bool isDebugPrintOn = true;
        
        private void Awake()
        {
            IAtfInitializable[] allSystems = {
                AtfQueueBasedRecorder.Instance,
                AtfDictionaryBasedActionStorage.Instance,
                AtfFileSystemBasedIntegrator.Instance,
                AtfPlayerPrefsBasedActionStorageSaver.Instance
            };

            #region INITALIZATION OF ATF
            foreach (var i in allSystems)
            {
                i.Initialize();
            }
            DependencyInjector.Instance.Initialize("ATF");
            DependencyInjector.Instance.Inject();
            #endregion
        }
    }
}