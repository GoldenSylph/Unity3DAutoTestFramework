using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.DI;
using Bedrin.Helper;

namespace ATF.Recorder
{
    [Injectable]
    public class ATFCoroutineBasedRecorder : MonoSingleton<ATFCoroutineBasedRecorder>, IATFRecorder
    {
        private bool Recording;

        public void Initialize()
        {
            Recording = false;
        }

        public bool IsPlaying()
        {
            return !Recording;
        }

        public bool IsRecording()
        {
            return Recording;
        }

        public void PauseRecord(string recordName)
        {
            throw new NotImplementedException();
        }

        public void PlayRecord(string recordName)
        {
            throw new NotImplementedException();
        }

        public void StartRecord(string recordName)
        {
            throw new NotImplementedException();
        }

        public void StopRecord(string recordName)
        {
            throw new NotImplementedException();
        }
    }
}
