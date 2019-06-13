using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Bedrin.Helper;
using Bedrin.DI;

namespace ATF.Recorder
{
    [Injectable]
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
