using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.DI;
using Bedrin.Helper;
using UnityEngine;
using UnityEngine.Serialization;
using Action = ATF.Scripts.Storage.Action;

namespace ATF.Scripts.Recorder
{
    [Injectable]
    public class ATFQueueBasedRecorder : MonoSingleton<ATFQueueBasedRecorder>, IATFRecorder
    {
        [Inject(typeof(ATFDictionaryBasedActionStorage))]
        public readonly static IATFActionStorage STORAGE;

        [Header("Debug Settings:")]
        
        [SerializeField]
        private bool recording;

        [SerializeField]
        private bool playing;

        [SerializeField]
        private bool recordingPaused;

        [SerializeField]
        private bool playPaused;

        [SerializeField]
        private string currentRecordingName;

        public string GetCurrentRecordingName()
        {
            return currentRecordingName ?? (currentRecordingName = "DefaultRecord");
        }

        public void SetCurrentRecordingName(string value)
        {
            currentRecordingName = value;
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
            return playing;
        }

        public bool IsRecording()
        {
            return recording;
        }

        public void SetRecording(bool value)
        {
            recording = value;
        }

        public void SetPlaying(bool value)
        {
            playing = value;
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
            if (!STORAGE.PrepareToPlayRecord(GetCurrentRecordingName())) return;
            SetRecording(false);
            SetPlaying(true);
        }

        public void StartRecord()
        {
            SetRecording(true);
            SetPlaying(false);
        }

        public void Record(FakeInput kind, object input, object fakeInputParameter)
        {
            STORAGE.Enqueue(GetCurrentRecordingName(), kind, fakeInputParameter, new Action { content = input });
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
            return recordingPaused;
        }

        public bool IsPlayPaused()
        {
            return playPaused;
        }

        public void SetRecordingPaused(bool value)
        {
            recordingPaused = value;
        }

        public void SetPlayPaused(bool value)
        {
            playPaused = value;
        }
    }
}
