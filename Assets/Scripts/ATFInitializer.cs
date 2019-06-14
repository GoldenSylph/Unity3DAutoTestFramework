using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.DI;
using ATF.Recorder;
using ATF.Storage;

namespace ATF
{
    public class ATFInitializer : MonoBehaviour
    {
        private void Awake()
        {
            ATFCoroutineBasedRecorder.Instance.Initialize();
            ATFDictionaryBasedActionStorage.Instance.Initialize();
            DependencyInjector.Instance.Initialize("ATF");
            DependencyInjector.Instance.Inject();
        }
    }
}