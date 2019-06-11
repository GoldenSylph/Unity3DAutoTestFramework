using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ATF.Helper;
using ATF.DI;

namespace ATF.Recorder
{
    [ATFInjectable]
    public abstract class ATFRecorder : MonoSingleton<ATFRecorder>
    {
        [Header("General Settings:")]
        public bool Recording;
        public abstract void StartRecord(string recordName);
        public abstract void PlayRecord(string recordName);
        public abstract void PauseRecord(string recordName);
        public abstract void StopRecord(string recordName);
    }
}
