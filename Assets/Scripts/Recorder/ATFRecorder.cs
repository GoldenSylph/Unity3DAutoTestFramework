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

        void PlayRecord(string recordName);
        void StartRecord(string recordName);

        void PauseRecord();
        void StopRecord();

        void SetRecording(bool value);

        string GetCurrentRecordingName();
        void Record(object input);
    }
}
