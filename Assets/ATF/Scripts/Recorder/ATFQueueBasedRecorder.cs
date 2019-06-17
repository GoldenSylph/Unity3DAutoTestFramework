using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.DI;
using Bedrin.Helper;
using ATF.Storage;

namespace ATF.Recorder
{
    [Injectable]
    public class ATFQueueBasedRecorder : MonoSingleton<ATFQueueBasedRecorder>, IATFRecorder
    {
        [Inject(typeof(ATFDictionaryBasedActionStorage))]
        public readonly static IATFActionStorage STORAGE;

        [Header("Debug Setttings:")]

        [SerializeField]
        private bool Recording;

        [SerializeField]
        private bool Playing;

        [SerializeField]
        private bool RecordingPaused;

        [SerializeField]
        private bool PlayPaused;


        [SerializeField]
        private string CurrentRecordingName;

        public string GetCurrentRecordingName()
        {
            if (CurrentRecordingName == null)
            {
                CurrentRecordingName = "DefaultRecord";
            }
            return CurrentRecordingName;
        }

        public void SetCurrentRecordingName(string value)
        {
            CurrentRecordingName = value;
        }

        public void Initialize()
        {
            SetRecording(false);
            SetPlaying(false);
            SetRecordingPaused(false);
            SetPlayPaused(false);
        }

        public bool IsPlaying()
        {
            return Playing;
        }

        public bool IsRecording()
        {
            return Recording;
        }

        public void SetRecording(bool value)
        {
            Recording = value;
        }

        public void SetPlaying(bool value)
        {
            Playing = value;
        }

        public void PauseRecord()
        {
            SetRecordingPaused(true);
        }

        public void ContinueRecord()
        {
            SetRecordingPaused(false);
        }

        public void PlayRecord()
        {
            if (STORAGE.PrepareToPlayRecord(GetCurrentRecordingName()))
            {
                SetRecording(false);
                SetPlaying(true);
            }
        }

        public void StartRecord()
        {
            SetRecording(true);
            SetPlaying(false);
        }

        public void Record(FakeInput kind, object input)
        {
            STORAGE.Enqueue(GetCurrentRecordingName(), kind, new Storage.Action() { content = input });
        }

        public void StopRecord()
        {
            SetRecording(false);
            SetPlaying(false);
            SetRecordingPaused(false);
        }

        public void PausePlay()
        {
            SetPlayPaused(true);
        }

        public void ContinuePlay()
        {
            SetPlayPaused(false);
        }

        public void StopPlay()
        {
            SetPlaying(false);
            SetRecording(false);
            STORAGE.ClearPlayStorage();
            SetPlayPaused(false);
        }

        public bool IsRecordingPaused()
        {
            return RecordingPaused;
        }

        public bool IsPlayPaused()
        {
            return PlayPaused;
        }

        public void SetRecordingPaused(bool value)
        {
            RecordingPaused = value;
        }

        public void SetPlayPaused(bool value)
        {
            PlayPaused = value;
        }
    }
}
