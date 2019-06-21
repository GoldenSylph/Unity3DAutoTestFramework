using System.Collections;
using System.Collections.Generic;
using ATF.Scripts.Integration;
using UnityEngine;
using Bedrin.DI;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;

namespace ATF
{
    public class ATFInitializer : MonoBehaviour
    {
        private void Awake()
        {
            IATFInitializable[] ALL_SYSTEMS = {
                ATFQueueBasedRecorder.Instance,
                ATFDictionaryBasedActionStorage.Instance,
                ATFFileSystemBasedIntegrator.Instance,
                ATFPlayerPrefsBasedActionStorageSaver.Instance
            };

            #region INITALIZATION OF ATF
            foreach (IATFInitializable i in ALL_SYSTEMS)
            {
                i.Initialize();
            }
            DependencyInjector.Instance.Initialize("ATF");
            DependencyInjector.Instance.Inject();
            #endregion
        }
    }
}