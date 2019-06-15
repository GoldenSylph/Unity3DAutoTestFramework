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
    public class ATFCoroutineBasedRecorder : MonoSingleton<ATFCoroutineBasedRecorder>, IATFRecorder
    {
        [Inject(typeof(ATFDictionaryBasedActionStorage))]
        public readonly static IATFActionStorage STORAGE;

        [SerializeField]
        private bool Recording;

        [SerializeField]
        private string CurrentRecording;

        [SerializeField]
        private float CurrentStartRecordingTime;

        public string GetCurrentRecordingName()
        {
            return CurrentRecording = "Test recording";
        }

        public void Initialize()
        {
            SetRecording(false);
        }

        public bool IsPlaying()
        {
            return !Recording;
        }

        public bool IsRecording()
        {
            return Recording;
        }

        public void SetRecording(bool value)
        {
            Recording = value;
        }

        public void PauseRecord()
        {
            SetRecording(false);
        }

        public void PlayRecord(string recordName)
        {
            SetRecording(false);
        }

        public void StartRecord(string recordName)
        {
            SetRecording(true);
            CurrentStartRecordingTime = Time.deltaTime;
            foreach (FakeInput fin in Enum.GetValues(typeof(FakeInput)))
            {
                Storage.Action ac = new Storage.Action
                {
                    duration = CurrentStartRecordingTime
                };
                STORAGE.Enqueue(GetCurrentRecordingName(), fin, ac);
            }
        }

        public void Record(object input)
        {

        }

        public void StopRecord()
        {
            SetRecording(false);
        }
    }
}
