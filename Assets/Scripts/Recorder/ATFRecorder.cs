using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Bedrin.Helper;

namespace ATF.Recorder
{
    public interface IATFRecorder : IATFInitializable
    {
        bool IsRecording();
        bool IsPlaying();
        void StartRecord(string recordName);
        void PlayRecord(string recordName);
        void PauseRecord(string recordName);
        void StopRecord(string recordName);
    }
}
